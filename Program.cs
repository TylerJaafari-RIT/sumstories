using sumstories.elements;
using System;
using System.IO;

public class Program {

	class InputProcessor {
		static Element CreateElement() {
			throw new NotImplementedException();
		}

		static Element CreateElement(string name) {

		}
	}

	static void Main(string [] args) {
		bool shutdown = false;

		while(!shutdown) {
			var input = Console.ReadLine();

			if (input == null) continue;

			args = input.Split(" ");

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
				switch(args[0]) {
					case("create"):

						break;
				}
			}
		}
	}
}