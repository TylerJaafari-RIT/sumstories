using Microsoft.Extensions.Configuration;
using Npgsql;
using sumstories.elements;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Program {
	public class Account(long ID, string Username, string Email, string Salt, string SessionKey) {
		public long ID { get; } = ID;
		public string Username { get; set; } = Username;
		public string Email { get; set; } = Email;
		public string Salt { get; set; } = Salt;
		public string SessionKey { get; set; } = SessionKey;
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

	static int idCounter = 1;

	static Folder TheEverythingFolder = new Folder(ID: 0, "All Elements");
	static Dictionary<int, Element> Elements = new Dictionary<int, Element>() { { 0, TheEverythingFolder } };
	// more dictionaries for each table
	/////////////////////////////////////////////////////////////
	
	static void AddElement(Element element) {
		TheEverythingFolder.AddItem(element);
		Elements.Add(idCounter, element);
		idCounter++;
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

	static void LoadUserData(Account user) {
		// should probably make this a static variable
		string[] tables = [""];
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
					}
					if (args.Length == 1) {
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
					string type = args[1].ToLower();
					if (type == "element") {
						// create element <category> <name>
						if (args.Length > 2 && Category.Defaults.TryGetValue(args[2].ToLower(), out Category? category)) {
							StorySumthing sumthing = new StorySumthing(idCounter, category);
							if (args.Length > 3) {
								sumthing.Name = args[3];
							}
							AddElement(sumthing);
						} else {
							Console.WriteLine("Usage: create element <category> <name>\n" + helpMsgNoCategory);
						}
					} else if (type == "folder") {
						// create folder <category> <name>
						if (args.Length > 2 && Category.Defaults.TryGetValue(args[2].ToLower(), out Category? category)) {
							Folder folder = new Folder(idCounter, category);
							if (args.Length > 3) {
								folder.Name = args[3];
							}
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
					} else if (int.TryParse(args[1], out int folderID) && Elements.TryGetValue(folderID, out Element? element) && element is Folder folder) {
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
					if (int.TryParse(item, out int id)) {
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
					if (int.TryParse(args[1], out int id) && Elements.TryGetValue(id, out Element? element) && element is StorySumthing sumthing) {
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
							if (args[0].Equals("num")) {
								string attributeName = args[1];
								int value = int.Parse(args[2]);

								// check if the attribute already exists; if not, create a new one
								bool attributeExists = sumthing.HasAttribute(attributeName);
								NumberAttribute numAttribute;
								if (attributeExists) numAttribute = (NumberAttribute)sumthing.GetAttribute(attributeName);
								else numAttribute = new NumberAttribute(attributeName);
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
								if (!attributeExists)
									sumthing.AddAttribute(numAttribute);
							} else if (args[0].Equals("text")) {
								string attributeName = args[1];
								string value = args[2];

								// check if attribute exists, you get the idea now
								bool attributeExists = sumthing.HasAttribute(attributeName);
								TextAttribute textAttribute;
								if (attributeExists) textAttribute = (TextAttribute)sumthing.GetAttribute(attributeName);
								else textAttribute = new TextAttribute(attributeName);
								textAttribute.Value = value;
								if (!attributeExists)
									sumthing.AddAttribute(textAttribute);
							} else if (args[0].Equals("exit") || args[0].Equals("done")) {
								doneEditing = true;
							} else if (args[0].Equals("delete")) {
								sumthing.RemoveAttribute(args[1]);
							} else if (args[0].Equals("help")) {
								Console.WriteLine("For Number Attributes: num <attributename> <value> <unit> [exact/approx/range] <maxvalue>\n" +
									"For Text Attributes: text <attributename> <value>\n" +
									"When finished, enter \"exit\" or \"done\".");
							} else {
								Console.WriteLine("Invalid attribute type.");
							}
						} while (!doneEditing);
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
					if (int.TryParse(args[1], out int id)) {
						if (Elements.TryGetValue(id, out Element? element)) {
							Console.WriteLine(element);
						} else { Console.WriteLine($"No element of ID {id} found."); }
					} else { Console.WriteLine("Invalid input."); }
					break;
				}
				case ("addto"): {
					// addto <folder id> <element id>
					if (args.Length < 3) {
						Console.WriteLine("Usage: addto <folder id> <element id>");
						break;
					}
					if (int.TryParse(args[1], out int folderID) && int.TryParse(args[2], out int elementID)) {
						if (Elements.TryGetValue(folderID, out Element? possibleFolder) && possibleFolder is Folder folder) {
							if (Elements.TryGetValue(elementID, out Element? element)) {
								folder.AddItem(element);
								Console.WriteLine($"Added {element.Name} to folder {folder.Name}");
							} else { Console.WriteLine($"No element of ID {elementID} found."); }
						} else { Console.WriteLine($"ID {folderID} does not correspond to an existing folder."); }
					} else { Console.WriteLine("Invalid input."); }
					break;
				}
				case ("removefrom"): {
					// removefrom <folder id> <element id>
					if (args.Length < 3) {
						Console.WriteLine("Usage: removefrom <folder id> <element id>");
						break;
					}
					if (int.TryParse(args[1], out int folderID) && int.TryParse(args[2], out int elementID)) {
						if (folderID == 0) {
							Console.WriteLine("Cannot remove items from the main folder.");
							break;
						} else if (Elements.TryGetValue(folderID, out Element? possibleFolder) && possibleFolder is Folder theFolder) {
							theFolder.RemoveItem(elementID);
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