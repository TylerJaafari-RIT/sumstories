using System;
using System.IO;

public class Program {

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
			 *  
			 */
		}
	}
}