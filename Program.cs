using sumstories.elements;
using System;
using System.IO;

public class Program {

	// might not need this
	class InputProcessor {
		static Element CreateElement() {
			throw new NotImplementedException();
		}

		static Element CreateElement(string name) {
			throw new NotImplementedException();
		}
	}

	///////////////////////// FACTORIES /////////////////////////
	/// Folders
	static ElementFactory FolderFactory = new ElementFactory();

	// Elements
	/////////////////////////////////////////////////////////////

	static void Main(string [] args) {
		bool shutdown = false;

		while (!shutdown) {
			var input = Console.ReadLine();

			if (input == null) continue;

			args = input.ToLower().Split(" ");

			/// The commands implemented here will form the basis for the REST API
			/** CORE COMMANDS WE MUST IMPLEMENT:
			 *  - Create element (includes folder)
			 *  - Delete element
			 *  - Edit element
			 *  - List elements
			 */
			int minArgs = 2;
			if (args.Length < minArgs) {
				Console.WriteLine("Insufficient arguments provided.");
				continue;
			} else {
				switch (args[0]) {
					case ("create"):
						string type = args[1];
						if (type == "element") {
							// create element
						} else if (type == "folder") {
							// create folder
						}
						break;
				}
			}
		}
	}
}