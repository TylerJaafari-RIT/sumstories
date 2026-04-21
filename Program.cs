using Microsoft.Extensions.Configuration;
using Npgsql;
using sumstories.elements;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

public class Program {
	public class Account(long ID, string Username, string Email, string Salt, string SessionKey) {
		public long ID { get; } = ID;
		public string Username { get; set; } = Username;
		public string Email { get; set; } = Email;
		public string Salt { get; set; } = Salt;
		public string SessionKey { get; set; } = SessionKey;
	}

	static bool CheckLogin(Account? user, NpgsqlConnection connection) {
		if (user is null) return false;
		string sqlText = "SELECT session_key FROM accounts WHERE id = @id";

		using var cmd = new NpgsqlCommand(sqlText, connection);
		cmd.Parameters.AddWithValue("id", user.ID);
		using var reader = cmd.ExecuteReader();
		bool check = false;
		if (reader.Read()) {
			check = user.SessionKey.Equals(reader.GetString(0));
		}
		return check;
	}

	static JsonSerializerOptions jsonOptions = new JsonSerializerOptions {
		PropertyNameCaseInsensitive = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	static List<IAttribute> ParseAttributeJson(string json) {
		List<IAttribute.DbValues> attributeVals = JsonSerializer.Deserialize<List<IAttribute.DbValues>>(json, jsonOptions);
		List<IAttribute> attributes = new List<IAttribute>();
		foreach (IAttribute.DbValues attributeVal in attributeVals) {
			if (attributeVal.type == (int)IAttribute.Type.TEXT) {
				TextAttribute text = new TextAttribute(attributeVal.id, attributeVal.name, attributeVal.text_value);
				attributes.Add(text);
			} else if (attributeVal.type == (int)IAttribute.Type.NUMBER && attributeVal.num_value != null) {
				NumberAttribute num = new NumberAttribute(attributeVal.id, attributeVal.name, (int)attributeVal.num_value);
				num.Unit = attributeVal.text_value;
				if (attributeVal.accuracy == (int)Accuracy.Range) {
					num.Accuracy = Accuracy.Range;
					num.MaxValue = attributeVal.maximum_value;
				} else if (attributeVal.accuracy == (int)Accuracy.Approximate) {
					num.Accuracy = Accuracy.Approximate;
				}
				attributes.Add(num);
			}
		}
		return attributes;
	}

	static void LoadUserData(Account user, NpgsqlConnection connection) {
		// should probably make this a static variable
		string sqlText;


		// Load StorySumthings
		sqlText = """
			SELECT sumthings.id, sumthings.name, categories.name category, sumthings.last_updated,
				json_agg(attributes.*) FILTER (WHERE attributes.id IS NOT NULL) AS attributes
				FROM sumthings
				LEFT JOIN categories ON categories.id = sumthings.category
				LEFT JOIN LATERAL unnest(sumthings.attributes) as attr_id ON true
				LEFT JOIN attributes ON attributes.id = attr_id
				WHERE sumthings.account = @account_id
				GROUP BY sumthings.id, categories.name
			""";

		// this should return a resulting table that looks like this:
		// id | name | category | last_updated | attributes
		// the attributes column holds JSON representations of all of that sumthing's attributes
		using (var cmd = new NpgsqlCommand(sqlText, connection)) {
			cmd.Parameters.AddWithValue("account_id", user.ID);
			using var reader = cmd.ExecuteReader();
			while (reader.Read()) {
				long id = reader.GetInt64(0);
				string name = reader.GetString(1);
				//long categoryId = reader.GetInt64(2);
				string categoryName = reader.GetString(2);
				// TODO: add LastUpdated field to StorySumthing
				DateTime lastUpdated = reader.GetDateTime(3);
				List<IAttribute> attributes = new List<IAttribute>();
				if (!reader.IsDBNull(4))
					attributes = ParseAttributeJson(reader.GetString(4));
				// only checks against default categories. will need additional logic for custom categories
				// TODO: add logic for custom categories
				if (Category.Defaults.TryGetValue(categoryName, out Category? category) && category != null) {
					StorySumthing sumthing = new StorySumthing(id, name, category, attributes);
					AddElement(sumthing);
				} else {
					Console.WriteLine(helpMsgCategoryNotFound, categoryName);
				}
			}
		}

		// Load Folders
		sqlText = """
			SELECT folders.id, folders.name, categories.name category, items FROM folders
			LEFT JOIN categories on categories.id = folders.category
			WHERE account = @account_id
			""";
			

		using (var cmd = new NpgsqlCommand(sqlText, connection)) {
			cmd.Parameters.AddWithValue("account_id", user.ID);
			using var reader = cmd.ExecuteReader();
			while (reader.Read()) {
				long id = reader.GetInt64(0);
				string name = reader.GetString(1);
				string categoryName = reader.GetString(2);
				long[] itemIDs = reader.IsDBNull(3) ? [] : reader.GetFieldValue<long[]>(3);

				if (Category.Defaults.TryGetValue(categoryName, out Category? category) && category != null) {
					Folder folder = new Folder(id, name, category);
					foreach (long itemID in itemIDs) {
						if (Elements.TryGetValue(itemID, out Element? element)) {
							folder.AddItem(element);
						}
					}
					AddElement(folder);
				} else {
					Console.WriteLine(helpMsgCategoryNotFound, categoryName);
				}
			}
		}

		// Load SubFolders
		sqlText = """
			SELECT folders.id, subfolders FROM folders
			WHERE account = @account_id
			""";

		using (var cmd = new NpgsqlCommand(sqlText, connection)) {
			cmd.Parameters.AddWithValue("account_id", user.ID);
			using var reader = cmd.ExecuteReader();
			while (reader.Read()) {
				long id = reader.GetInt64(0);
				long[] subfolderIDs = reader.IsDBNull(1) ? [] : reader.GetFieldValue<long[]>(1);
				Folder folder = Folders[id];
				foreach (long subfolderID in subfolderIDs) {
					if (subfolderID == id) {
						Console.WriteLine($"Error: Folder {id} contains a reference to itself in subfolders.");
						continue;
					}
					if (Folders.TryGetValue(subfolderID, out Folder? subfolder)) {
						folder.AddItem(subfolder);
					}
				}
			}
		}
	}

	static long[] GetAttributeIDs(IAttribute[] attributes) {
		long[] attIds = new long[attributes.Length];
		for (int i = 0; i < attIds.Length; i++) {
			attIds[i] = attributes[i].ID;
		}
		return attIds;
	}

	static long[] GetAttributeIDs(List<IAttribute> attributes) {
		long[] attIds = new long[attributes.Count];
		for (int i = 0; i < attIds.Length; i++) {
			attIds[i] = attributes[i].ID;
		}
		return attIds;
	}

	// TODO: Migrate to server (or should it be client at this point?) class
	static IAttribute[] InsertAttributes(IAttribute[] attributes, Account user, NpgsqlConnection connection) {
		if (user is null) throw new ArgumentNullException(nameof(user), "User is not logged in.");

		var uploaded = new List<IAttribute>();
		if (attributes == null || attributes.Length == 0) return uploaded.ToArray();

		// build a batch with one INSERT ... RETURNING id per attribute
		using var batch = new NpgsqlBatch(connection);

		foreach (var att in attributes) {
			var sqlText = "INSERT INTO attributes (account, name, type, num_value, text_value, accuracy, maximum_value) " +
						  "VALUES (@account, @name, @type, @num_value, @text_value, @accuracy, @maximum_value) RETURNING id";

			var batchCmd = new NpgsqlBatchCommand(sqlText);
			batchCmd.Parameters.AddWithValue("account", user.ID);
			batchCmd.Parameters.AddWithValue("name", att.Name);

			if (att is NumberAttribute n) {
				batchCmd.Parameters.AddWithValue("type", (int)IAttribute.Type.NUMBER);
				batchCmd.Parameters.AddWithValue("num_value", n.Value);
				batchCmd.Parameters.AddWithValue("text_value", n.Unit);
				//batchCmd.Parameters.AddWithValue("unit", n.Unit);
				batchCmd.Parameters.AddWithValue("accuracy", (int)n.Accuracy);
				batchCmd.Parameters.AddWithValue("maximum_value", n.MaxValue.HasValue ? n.MaxValue.Value : DBNull.Value);
			} else if (att is TextAttribute t) {
				batchCmd.Parameters.AddWithValue("type", (int)IAttribute.Type.TEXT);
				batchCmd.Parameters.AddWithValue("num_value", DBNull.Value);
				batchCmd.Parameters.AddWithValue("text_value", t.Value);
				//batchCmd.Parameters.AddWithValue("unit", DBNull.Value);
				batchCmd.Parameters.AddWithValue("accuracy", DBNull.Value);
				batchCmd.Parameters.AddWithValue("maximum_value", DBNull.Value);
			} else {
				// Fallback: store as blank text attribute
				batchCmd.Parameters.AddWithValue("type", (int)IAttribute.Type.TEXT);
				batchCmd.Parameters.AddWithValue("num_value", DBNull.Value);
				batchCmd.Parameters.AddWithValue("text_value", att.Name);
				batchCmd.Parameters.AddWithValue("unit", DBNull.Value);
				batchCmd.Parameters.AddWithValue("accuracy", DBNull.Value);
				batchCmd.Parameters.AddWithValue("maximum_value", DBNull.Value);
			}

			batch.BatchCommands.Add(batchCmd);
		}

		// Execute batch and read results. Each command produces one result set (the returned id).
		using var reader = batch.ExecuteReader();
		foreach (var attr in attributes) {
			if (!reader.Read())
				throw new InvalidOperationException("Expected a row for each batch command.");

			long dbId = reader.GetInt64(0); // RETURNING id -> read first column
			uploaded.Add(attr.Clone(dbId));

			// advance to the next command's result set
			reader.NextResult();
		}

		return uploaded.ToArray();
	}

	static bool UpdateAttribute(IAttribute attribute, Account user, NpgsqlConnection connection) {
		if (user is null) throw new ArgumentNullException(nameof(user), "User is not logged in.");
		var sqlText = "UPDATE attributes SET name = @name, type = @type, num_value = @num_value, " +
			"text_value = @text_value, accuracy = @accuracy, " +
			"maximum_value = @maximum_value WHERE id = @id RETURNING id";
		using var cmd = new NpgsqlCommand(sqlText, connection);
		cmd.Parameters.AddWithValue("id", attribute.ID);
		//cmd.Parameters.AddWithValue("account", user.ID);
		cmd.Parameters.AddWithValue("name", attribute.Name);
		if (attribute is NumberAttribute n) {
			cmd.Parameters.AddWithValue("type", (int)IAttribute.Type.NUMBER);
			cmd.Parameters.AddWithValue("num_value", n.Value);
			cmd.Parameters.AddWithValue("text_value", n.Unit);
			cmd.Parameters.AddWithValue("accuracy", (int)n.Accuracy);
			cmd.Parameters.AddWithValue("maximum_value", n.MaxValue.HasValue ? n.MaxValue.Value : DBNull.Value);
		} else if (attribute is TextAttribute t) {
			cmd.Parameters.AddWithValue("type", (int)IAttribute.Type.TEXT);
			cmd.Parameters.AddWithValue("num_value", DBNull.Value);
			cmd.Parameters.AddWithValue("text_value", t.Value);
			cmd.Parameters.AddWithValue("accuracy", DBNull.Value);
			cmd.Parameters.AddWithValue("maximum_value", DBNull.Value);
		}

		using var reader = cmd.ExecuteReader();
		if (!reader.Read()) {
			Console.WriteLine($"No existing attribute of ID {attribute.ID} was found.");
			return false;
		}
		return true;
	}

	static bool DeleteAttribute(IAttribute target, Account user, NpgsqlConnection connection) {
		if (user is null) throw new ArgumentNullException(nameof(user), "User is not logged in.");
		var sqlText = "DELETE FROM attributes WHERE id = @id RETURNING id";
		
		using var cmd = new NpgsqlCommand(sqlText, connection);
		cmd.Parameters.AddWithValue("id", target.ID);
		
		using var reader = cmd.ExecuteReader();
		if (!reader.Read()) {
			Console.WriteLine($"No existing attribute of ID {target.ID} was found.");
			return false;
		} else {
			Console.WriteLine($"Attribute {reader.GetInt64(0)} deleted from database.");
			return true;
		}
	}

	static bool UpdateStorySumthing(StorySumthing sumthing, Account user, NpgsqlConnection connection) {
		if (user is null) throw new ArgumentNullException(nameof(user), "User is not logged in.");
		var sqlText = "UPDATE sumthings SET name = @name, category = @category, " +
			"attributes = @attributes, last_updated = @last_updated WHERE id = @id";
		
		using var cmd = new NpgsqlCommand(sqlText, connection);
		cmd.Parameters.AddWithValue("id", sumthing.ID);
		cmd.Parameters.AddWithValue("name", sumthing.Name);
		cmd.Parameters.AddWithValue("category", sumthing.Category.ID);
		cmd.Parameters.AddWithValue("attributes", GetAttributeIDs(sumthing.Attributes));
		cmd.Parameters.AddWithValue("last_updated", DateTime.UtcNow);

		var result = cmd.ExecuteNonQuery();

		return result != -1;
	}

	static bool UpdateFolder(Folder folder, Account user, NpgsqlConnection connection) {
		var sqlText = "UPDATE folders SET items = @items, subfolders = @subfolders WHERE id = @id RETURNING id";
		using var command = new NpgsqlCommand(sqlText, connection);
		command.Parameters.AddWithValue("id", folder.ID);
		List<long> itemIDs = new List<long>();
		List<long> subfolderIDs = new List<long>();
		foreach (Element item in folder.Items) {
			if (item is StorySumthing)
				itemIDs.Add(item.ID);
			else if (item is Folder)
				subfolderIDs.Add(item.ID);
		}
		command.Parameters.AddWithValue("items", itemIDs);
		command.Parameters.AddWithValue("subfolders", subfolderIDs);

		using var reader = command.ExecuteReader();

		return reader.Read();
	}

	static string BuildConnectionString(IConfigurationSection config) {
		NpgsqlConnectionStringBuilder builder = new () {
			Database = config["Database"],
			Host = config["Host"],
			Port = int.Parse(config["Port"]),
			Username = config["Username"],
			Password = config["Password"]
		};

		return builder.ConnectionString;
	}

	static string GenerateSalt() {
		var rng = RandomNumberGenerator.Create();
		byte[] random = new byte[64];
		rng.GetBytes(random);
		byte[] saltHash = SHA512.HashData(random);
		return Convert.ToBase64String(saltHash);
	}

	// Compute SHA-512(salt || password) and return Base64 string.
	static string ComputeHash(string password, string salt) {
		byte[] saltyBytes = Convert.FromBase64String(salt);
		byte[] pwdBytes = Encoding.UTF8.GetBytes(password);

		byte[] combined = new byte[saltyBytes.Length + pwdBytes.Length];
		Buffer.BlockCopy(saltyBytes, 0, combined, 0, saltyBytes.Length);
		Buffer.BlockCopy(pwdBytes, 0, combined, saltyBytes.Length, pwdBytes.Length);

		byte[] hash = SHA512.HashData(combined);
		return Convert.ToBase64String(hash);
	}

	static string GenerateSessionKey(int bytes = 32) {
		byte[] data = RandomNumberGenerator.GetBytes(bytes);

		string base64 = Convert.ToBase64String(data)
			.Replace('+', '-')
			.Replace('/', '_')
			.TrimEnd('=');

		return base64;
	}

	////////////////////////// FIELDS ///////////////////////////
	private const string helpMsgNoCategory = "If uncategorized, pass \'none\' to category argument. Name is optional.";
	private const string helpMsgCategoryNotFound = "Category \"{0}\" does not exist.";

	//static long idCounter = 1;

	static Folder TheEverythingFolder = new Folder(ID: 0, "All Elements");
	static Dictionary<long, Element> Elements = new Dictionary<long, Element>() { { 0, TheEverythingFolder } };
	static Dictionary<long, Folder> Folders = new Dictionary<long, Folder>() { { 0, TheEverythingFolder } };
	// more dictionaries for each table
	/////////////////////////////////////////////////////////////
	
	static void AddElement(Element element) {
		TheEverythingFolder.AddItem(element);
		if (element is StorySumthing sumthing)
			Elements.Add(element.ID, sumthing);
		else if (element is Folder folder)
			Folders.Add(element.ID, folder);
	}

	static void PrintTabs(int level) {
		for (int i = 0; i < level; i++) {
			Console.Write("\t");
		}
	}

	static void PrintFolderContents(Folder folder, int level) {
		Console.Write(folder.ID);
		PrintTabs(level);
		Console.WriteLine(folder.Name);
		foreach (Element element in folder.Items) {
			if (element is Folder subfolder) {
				//PrintTabs(level + 1);
				//Console.WriteLine(element.Name);
				PrintFolderContents(subfolder, level + 1);
			} else {
				Console.Write(element.ID);
				PrintTabs(level + 1);
				Console.WriteLine(element.Name);
			}
		}
	}

	static string[] SplitWithQuotes(string input) {
		string[] quoteSplit = input.Split("\"");
		List<string> fullSplit = [];
		for (int i = 0; i < quoteSplit.Length; i++) {
			if (i % 2 == 0) {
				foreach (string s in quoteSplit[i].Split(" ", StringSplitOptions.RemoveEmptyEntries)) {
					fullSplit.Add(s);
				}
			} else {
				fullSplit.Add(quoteSplit[i]);
			}
		}
		return fullSplit.ToArray();
	}

	static async Task Main(string [] args) {
		Console.WriteLine("SumStories App Version 0.1");
		Console.WriteLine("Copyright (c) 2026 Tyler Jaafari. All rights reserved.");
		bool shutdown = false;

		var configRoot = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false, true).Build();
		var dbConfig = configRoot.GetSection("Database");
		string connectionString = BuildConnectionString(dbConfig);

		await using var connection = new NpgsqlConnection(connectionString);

		//connection.Open(); // this is also usable for a non-asynchronous approach
		await connection.OpenAsync();

		//string sql_command = "SELECT name FROM categories";
		//await using var command = new NpgsqlCommand(sql_command, connection);
		//await using var reader = await command.ExecuteReaderAsync();

		//while (await reader.ReadAsync()) {
		//	Console.WriteLine("Reading from DB...");
		//	Console.WriteLine(reader.GetString(0));
		//}

		Account? user = null;
		Console.WriteLine("Welcome. Please login or register.");

		while (!shutdown) {
			var input = Console.ReadLine();

			if (input == null || input.Trim() == "") continue;

			args = SplitWithQuotes(input);

			/// The commands implemented here will form the basis for the REST API
			/** CORE COMMANDS WE MUST IMPLEMENT:
			 *  - Create element (includes folder)
			 *  - Delete element
			 *  - Edit element
			 *  - List elements
			 *  - Add to/Remove from folder
			 */
			switch (args[0].ToLower()) {
				case ("register"): {
					if (args.Length != 4) {
						Console.WriteLine("Usage: register <email> <username> <password>");
						break;
					}

					string email = args[1];
					string username = args[2];
					string password = args[3];

					// first we check if the user already exists
					// email account recovery is probably a reach goal at this point
					// TODO: Migrate to server class
					string sqlText = "SELECT username, email FROM accounts WHERE username = @username OR email = @email";
					await using (var selectCmd = new NpgsqlCommand(sqlText, connection)) {
						selectCmd.Parameters.AddWithValue("username", username);
						selectCmd.Parameters.AddWithValue("email", email);
						await using var reader = await selectCmd.ExecuteReaderAsync();
						if (await reader.ReadAsync()) {
							if (reader.GetString(0).Equals(username, StringComparison.OrdinalIgnoreCase))
								Console.WriteLine("This username is already taken.");
							if (reader.GetString(1).Equals(email, StringComparison.OrdinalIgnoreCase))
								Console.WriteLine("This email is already in use.");
							break;
						}
					}
					// end TODO
					string salt = GenerateSalt();

					string hashedPassword = ComputeHash(password, salt);
					// TODO: Migrate to server class
					sqlText = "INSERT INTO accounts (username, email, password, salt) VALUES " +
							"(@username, @email, @password, @salt) RETURNING id";

					using var insertCmd = new NpgsqlCommand(sqlText, connection);
					insertCmd.Parameters.AddWithValue("username", username);
					insertCmd.Parameters.AddWithValue("email", email);
					insertCmd.Parameters.AddWithValue("password", hashedPassword);
					insertCmd.Parameters.AddWithValue("salt", salt);
					// end TODO

					await using (var reader = await insertCmd.ExecuteReaderAsync()) {
						if (await reader.ReadAsync()) {
							Console.WriteLine($"User created with ID {reader.GetInt64(0)}\n" +
								$"Please log in with your username and password");
						} else {
							Console.WriteLine("Error occurred creating new user.");
						}
					}

					break;
				}
				case ("login"): {
					if (args.Length != 3) {
						Console.WriteLine("Usage: login <username> <password>");
						break;
					}
					string username = args[1];
					string password = args[2];

					// TODO: Migrate to server class
					string sqlText = "SELECT id, username, email, salt, password FROM accounts WHERE username = @username LIMIT 1;";

					await using var cmd = new NpgsqlCommand(sqlText, connection);
					cmd.Parameters.AddWithValue("username", username);
					await using var reader = await cmd.ExecuteReaderAsync();
					if(!await reader.ReadAsync()) {
						Console.WriteLine("Username not found.");
						break;
					}
					// end TODO

					long dbId = reader.GetInt64(0);
					string dbUsername = reader.GetString(1);
					string dbEmail = reader.GetString(2);
					string dbSalt = reader.IsDBNull(3) ? "" : reader.GetString(3);
					string hashedPassword = reader.IsDBNull(4) ? "" : reader.GetString(4);

					await reader.CloseAsync();

					// verify password
					//bool verified = false;
					if (!string.IsNullOrEmpty(dbSalt) && !string.IsNullOrEmpty(hashedPassword)) {
						string hash = ComputeHash(password, dbSalt);
						byte[] inputBytes = Convert.FromBase64String(hash);
						byte[] databaseBytes = Convert.FromBase64String(hashedPassword);

						string sessionKey = GenerateSessionKey();

						if (CryptographicOperations.FixedTimeEquals(inputBytes, databaseBytes)) {
							user = new Account(dbId, dbUsername, dbEmail, dbSalt, sessionKey);
							// TODO: Migrate to server class
							// inserting directly into string should be fine here since these are not user inputs
							sqlText = $"UPDATE accounts SET session_key = '{sessionKey}' WHERE id = {dbId}";
							await using (var updateCmd = new NpgsqlCommand(sqlText, connection)) {
								updateCmd.ExecuteNonQuery();
							}
							// end TODO
							Console.WriteLine($"Welcome, {dbUsername}!");
							LoadUserData(user, connection);
						} else {
							Console.WriteLine("Invalid username or password.");
						}
					} else {
						Console.WriteLine("No password or salt to compare to; something has gone terribly wrong!");
					}
					break;
				}
				case ("help"):
					if (user is null) {
						Console.WriteLine("login <username> <password> - attempt to log in to your account.\n" +
							"register <email> <username> <password> - register a new account.");
						break;
					} else if (args.Length == 1) {
						Console.WriteLine("create - make a new element or folder\n" +
							"list - list all elements and their IDs\n" +
							"delete - delete an element\n" +
							"edit - switch to edit mode on a specific element\n" +
							"search - list IDs of elements whose names contain a given string\n" +
							"show - display an element's ID, name, and attributes\n" +
							"addto - add an element or folder to a folder\n" +
							"removefrom - remove an element or folder from a folder\n" +
							"exit - end the program");
					}
					break;

				case ("create"): {
					if (!CheckLogin(user, connection)) {
						Console.WriteLine("Not logged in.");
						break;
					}
					if (args.Length < 2) {
						Console.WriteLine("Usage: create [element/folder] <category> <name>");
						break;
					}
					string type = args[1].ToLower();
					if (type == "element") {
						// create element <category> <name>
						// TODO: Refactor all references to default category dictionary to a cached storage which
						// is initialized with the defaults + custom categories when loading user data after login
						if (args.Length > 2 && Category.Defaults.TryGetValue(args[2].ToLower(), out Category? category)) {
							string sumthingName = (category == Category.NONE) ? "New Element" : "New " + category.Name;
							if (args.Length > 3) {
								sumthingName = args[3];
							}
							// TODO: Migrate to server class
							// we need to get the id generated by the server and get rid of the id counter altogether
							string sqlText = "INSERT INTO sumthings (account, name, category, attributes, last_updated) " +
															"VALUES (@account, @name, @category, @attributes, @last_updated) " +
															"RETURNING id";
							await using var cmd = new NpgsqlCommand(sqlText, connection);
							cmd.Parameters.AddWithValue("account", user.ID);
							cmd.Parameters.AddWithValue("name", sumthingName);
							cmd.Parameters.AddWithValue("category", category.ID);
							// Will executing more fetches while using an async variable cause problems? Let's find out
							IAttribute[] defaultAttributes = InsertAttributes(category.DefaultAttributes, user, connection);
							long[] attIds = new long[defaultAttributes.Length];
							for (int i = 0; i < attIds.Length; i++) {
								attIds[i] = defaultAttributes[i].ID;
							}
							cmd.Parameters.AddWithValue("attributes", attIds);
							cmd.Parameters.AddWithValue("last_updated", DateTime.UtcNow);
							await using var reader = await cmd.ExecuteReaderAsync();
							if (!reader.Read()) {
								Console.WriteLine("Error creating element; no ID returned from database.");
								break;
							}
							long id = reader.GetInt64(0);
							StorySumthing sumthing = new StorySumthing(id, sumthingName, category);
							AddElement(sumthing);
						} else {
							Console.WriteLine("Usage: create element <category> <name>\n" + helpMsgNoCategory);
						}
					} else if (type == "folder") {
						// create folder <category> <name>
						if (args.Length > 2 && Category.Defaults.TryGetValue(args[2].ToLower(), out Category? category)) {
							string folderName = "New Folder";
							if (args.Length > 3) {
								folderName = args[3];
							}
							string sqlText = "INSERT INTO folders (account, name, category) " +
															"VALUES (@account, @name, @category) " +
															"RETURNING id";
							await using var cmd = new NpgsqlCommand(sqlText, connection);
							cmd.Parameters.AddWithValue("account", user.ID);
							cmd.Parameters.AddWithValue("name", folderName);
							cmd.Parameters.AddWithValue("category", category.ID);
							await using var reader = await cmd.ExecuteReaderAsync();
							if (!reader.Read()) {
								Console.WriteLine("Error creating element; no ID returned from database.");
								break;
							}
							long id = reader.GetInt64(0);
							Folder folder = new Folder(id, folderName, category);
							AddElement(folder);
						} else {
							Console.WriteLine("Usage: create folder <category> <name>\n" + helpMsgNoCategory);
						}
					} else {
						Console.WriteLine("Usage: create [element/folder] <category> <name>");
					}
					break;
				}
				case ("list"): {
					if (args.Length == 1 || args[1].ToLower() == "all") {
						PrintFolderContents(TheEverythingFolder, level: 0);
                    } else if (long.TryParse(args[1], out long folderID) && Elements.TryGetValue(folderID, out Element? element) && element is Folder folder) {
						PrintFolderContents(folder, level: 0);
					}
					break;
				}
				case ("delete"): {
					if (args.Length < 2) {
						Console.WriteLine("Usage: delete <element id>");
						break;
					}
					string item = args[1].ToLower();
                    if (long.TryParse(item, out long id)) {
						if (Elements.TryGetValue(id, out Element? element)) {
							Console.Write($"Delete element [{id}] \"{element.Name}\"? (y/n) ");
							char response = (char)Console.Read();
							if (response == 'y') {
								foreach (Element thing in Elements.Values) {
									if (thing is Folder folder && folder.Items.Contains(element)) {
										folder.RemoveItem(element);
									}
								}
								Elements.Remove(id);
								Console.WriteLine("Element deleted.");
							}
						}
					}
					break;
				}
				case ("edit"): {
					// should enter another input loop that can be exited
					// exiting should prompt for saving/canceling (Y/N/C)
					// actually, scratch that.
					if (args.Length < 2) {
						Console.WriteLine("Usage: edit <element id>");
						break;
					}
                    if (long.TryParse(args[1], out long id) && Elements.TryGetValue(id, out Element? element) && element is StorySumthing sumthing) {
						bool doneEditing = false;
						// BEGIN LOOP
						// Let's shake things up with a do-while loop. I never use those.
						do {
							Console.WriteLine("----------------------------------------");
							Console.WriteLine(sumthing);
							Console.WriteLine("----------------------------------------");
							input = Console.ReadLine();
							if (input == null || input.Trim() == "") continue;
							// num <attributename> <value> "<unit>" [exact/approx/range] <maxvalue>
							// text <attributename> "<value>"
							args = SplitWithQuotes(input);
							// TODO: try/catch for the couple possible errors here. It may not be necessary
							// for the full app, but try adding some if there are weird errors later on
							if (args[0].Equals("num") && args.Length > 2) {
								string attributeName = args[1];
								int value = int.Parse(args[2]);

								// check if the attribute already exists; if not, create a new one
								bool attributeExists = sumthing.HasAttribute(attributeName);
								NumberAttribute numAttribute;
								if (attributeExists) numAttribute = (NumberAttribute)sumthing.GetAttribute(attributeName);
								else numAttribute = new NumberAttribute(0, attributeName);
								numAttribute.Value = value;

								string unit = "";
								if (args.Length > 3) unit = args[3];
								else if (attributeExists) unit = numAttribute.Unit;
								string accuracy = "";
								if (args.Length > 4) accuracy = args[4].ToLower();
								int maxvalue = 0;
								if (accuracy.Equals("range") && args.Length > 5) maxvalue = int.Parse(args[5]);
								numAttribute.Unit = unit;
								// keeping this code snippet for later, when strings can match the enums exactly
								// for convenience sake

								//if (Enum.TryParse(typeof(Accuracy), accuracy, out object? accuracyValue)) {
								//	numAttribute.Accuracy = (Accuracy)accuracyValue;
								//}
								switch (accuracy) {
									case ("exact"):
										//case ("0"):
										numAttribute.Accuracy = Accuracy.Exact;
										break;
									case ("approx"):
										//case ("1"):
										numAttribute.Accuracy = Accuracy.Approximate;
										break;
									case ("range"):
										//case ("2"):
										numAttribute.Accuracy = Accuracy.Range;
										if (maxvalue >= value) numAttribute.MaxValue = maxvalue;
										break;
									case (""):
										break;
									default:
										Console.WriteLine("Invalid accuracy parameter");
										break;
								}
								if (!attributeExists) {
									sumthing.AddAttribute(InsertAttributes([numAttribute], user, connection)[0]);
								} else {
									UpdateAttribute(numAttribute, user, connection);
								}
							} else if (args[0].Equals("text") && args.Length > 2) {
								string attributeName = args[1];
								string value = args[2];

								// check if attribute exists, you get the idea now
								bool attributeExists = sumthing.HasAttribute(attributeName);
								TextAttribute textAttribute;
								if (attributeExists) textAttribute = (TextAttribute)sumthing.GetAttribute(attributeName);
								else textAttribute = new TextAttribute(attributeName);
								textAttribute.Value = value;
								if (!attributeExists) {
									sumthing.AddAttribute(InsertAttributes([textAttribute], user, connection)[0]);
								} else {
									UpdateAttribute(textAttribute, user, connection);
								}
							} else if (args[0].Equals("exit") || args[0].Equals("done")) {
								doneEditing = true;
							} else if (args[0].Equals("delete") && args.Length > 1) {
								IAttribute? target = sumthing.RemoveAttribute(args[1]);
								if (target is not null) {
									DeleteAttribute(target, user, connection);
								}
							} else if (args[0].Equals("help")) {
								Console.WriteLine("For Number Attributes: num <attributename> <value> <unit> [exact/approx/range] <maxvalue>\n" +
									"For Text Attributes: text <attributename> <value>\n" +
									"When finished, enter \"exit\" or \"done\".");
							} else {
								Console.WriteLine("Invalid arguments.");
							}
						} while (!doneEditing);

						if (UpdateStorySumthing(sumthing, user, connection)) {
							Console.WriteLine("Changes saved to database.");
						} else {
							Console.WriteLine("Error saving changes to database; no rows were changed.");
						}
					}
					break;
				}
				case ("search"): {
					if (args.Length < 2) {
						Console.WriteLine("Usage: search <string>\n" +
							"If your search term has spaces, use double quotation marks (\")");
						break;
					}
					List<Element> matches = [];
					foreach (Element element in Elements.Values) {
						if (element.Name.Contains(args[1]))
							matches.Add(element);
					}
					if (matches.Count == 0) {
						Console.WriteLine($"No matches found for \"{args[1]}\"");
					} else if (matches.Count == 1) {
						Console.WriteLine(matches[0]);
					} else {
						Console.WriteLine("Multiple matches found:");
						foreach (Element match in matches) {
							Console.WriteLine($"[{match.ID}]\t{match.Name}");
						}
					}
					break;
				}
				case ("show"): {
					if (args.Length < 2) {
						Console.WriteLine("Usage: show <item>");
						break;
					}
					if (long.TryParse(args[1], out long id)) {
						if (Elements.TryGetValue(id, out Element? element)) {
							Console.WriteLine(element);
						} else { Console.WriteLine($"No element of ID {id} found."); }
					} else { Console.WriteLine("Invalid input."); }
					break;
				}
				case ("addto"): {
					// addto <folder id> <element id> (subfolder)
					if (args.Length < 3) {
						Console.WriteLine("Usage: addto <folder id> <element id> (subfolder)");
						break;
					}
					if (long.TryParse(args[1], out long folderID) && long.TryParse(args[2], out long elementID)) {
						if (Folders.TryGetValue(folderID, out Folder? folder)) {
							// check if there is a third argument. meant to be the word "subfolder" but can be literally anything.
							if (args.Length > 3 && folderID != elementID && Folders.TryGetValue(elementID, out Folder? subfolder)) {
								folder.AddItem(subfolder);
								if (UpdateFolder(folder, user, connection)) {
									Console.WriteLine($"Added {subfolder.Name} to folder {folder.Name}");
								} else { Console.WriteLine($"Error updating items list for folder {folderID}"); }
								// if we aren't adding a folder then we check the Elements dictionary instead
							} else if (Elements.TryGetValue(elementID, out Element? element)) {
								folder.AddItem(element);
								if (UpdateFolder(folder, user, connection)) {
									Console.WriteLine($"Added {element.Name} to folder {folder.Name}");
								} else { Console.WriteLine($"Error updating items list for folder {folderID}"); }
							} else { Console.WriteLine($"No element of ID {elementID} found."); }
						} else { Console.WriteLine($"ID {folderID} does not correspond to an existing folder."); }
					} else { Console.WriteLine("Invalid input."); }
					break;
				}
				case ("removefrom"): {
					// removefrom <folder id> <element id> (subfolder)
					if (args.Length < 3) {
						Console.WriteLine("Usage: removefrom <folder id> <element id> (subfolder)");
						break;
					}
                    if (long.TryParse(args[1], out long folderID) && long.TryParse(args[2], out long elementID)) {
						if (folderID == 0) {
							Console.WriteLine("Cannot remove items from the main folder.");
							break;
						}

						if (Folders.TryGetValue(folderID, out Folder? folder)) {
							string target = "";
							if (args.Length > 3 && folderID != elementID && Folders.TryGetValue(elementID, out Folder? subfolder)) {
								folder.RemoveItem(subfolder);
								target = $"folder {subfolder.Name}";
							} else if (Elements.TryGetValue(elementID, out Element? sumthing)) {
								folder.RemoveItem(sumthing);
								target = $"element {sumthing.Name}";
							}
							if (UpdateFolder(folder, user, connection)) {
								Console.WriteLine($"Removed {target}.");
							}
						} else { Console.WriteLine($"ID {folderID} does not correspond to an existing folder."); }
					} else { Console.WriteLine("Invalid input."); }
					break;
				}
				case ("exit"):
					shutdown = true;
					break;

				default:
					Console.WriteLine("Unknown command. Enter \'help\' for command list.");
					break;
			}
		}
	}
}