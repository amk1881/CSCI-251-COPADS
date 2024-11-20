
/*
CSCI 251 Project 3
Author: Anna Kurchenko
Date: 11/20/24
*/

using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Security.Cryptography;
using System.Diagnostics;


/*
Main Program functionality to handle prime/odd generation 
*/
class Program {
    
    // Main program that handles input processing 
    static void Main(string[] args) {
        try {
            if (args.Length < 1 ) {
                String msg = "Usage: <option> <other arguments> \n" +
                "option  -  'cipher', 'GenerateKeyStream' , 'Encrypt', 'Decrypt', 'MutlipleBits', 'EncryptImage', 'DecryptImage'\n" +
                "other arguments  -  option specific args\n";
                
                throw new ArgumentException(msg);
            }
            

            string option = args[0];
            //check valid option input 
            if (option != "cipher" && option != "GenerateKeyStream" && option != "Encrypt" && 
                option != "Decrypt" && option != "MutlipleBits" && option != "EncryptImage" && option != "DecryptImage" ) 
                
                throw new ArgumentException("'option' must be either 'odd' or 'prime'.");
                
            switch (option)
            {
                case "cipher":
                    if (args.Length != 3)
                        throw new ArgumentException("Usage for 'cipher': cipher <seed> <tap>\n");
                    string seed = args[1];
                    int tap = int.Parse(args[2]);
                    Cipher(seed, tap);
                    break;

                case "GenerateKeyStream":
                    if (args.Length != 4)
                        throw new ArgumentException("Usage for 'GenerateKeyStream': GenerateKeyStream <seed> <tap> <steps>\n");
                    seed = args[1];
                    tap = int.Parse(args[2]);
                    int steps = int.Parse(args[3]);
                    GenerateKeystream(seed, tap, steps);
                    break;

                // Other cases will be implemented later.
                default:
                    throw new ArgumentException($"Unsupported option '{option}'.");
            }
        }
        
        catch (ArgumentException ex) { 
            Console.WriteLine(ex.Message); 
        }
    }


    /**
    This option takes the initial seed and a tap position from the user and simulates one 
    step of the LFSR cipher. This returns and prints the new seed and the recent rightmost bit (which may 
    be 0 or 1)
    */
    static void Cipher(string seed, int tap)
    {
        if (seed.Length <= tap || tap < 0)
            throw new ArgumentException("Tap position must be within the length of the seed.");

        // Ensure the seed contains only 0s and 1s
        if (!IsBinaryString(seed))
            throw new ArgumentException("Seed must be a binary string (only 0s and 1s).");

        // XOR the bit at the tap position with the leftmost bit
        char newBit = (seed[0] == seed[tap] ? '0' : '1');

        // Shift left and append the new bit
        string newSeed = seed.Substring(1) + newBit;

        Console.WriteLine($"New Seed: {newSeed}");
        Console.WriteLine($"Recent Rightmost Bit: {newBit}");
    }

    /** Generates a keystream by simulating multiple steps of the LFSR cipher
        takes a seed, tap position, and the number of steps
    */
    static void GenerateKeystream(string seed, int tap, int steps)
    {
        if (seed.Length <= tap || tap < 0)
            throw new ArgumentException("Tap position must be within the length of the seed.");

        if (steps <= 0)
            throw new ArgumentException("Number of steps must be a positive integer.");

        // Ensure the seed contains only 0s and 1s
        if (!IsBinaryString(seed))
            throw new ArgumentException("Seed must be a binary string (only 0s and 1s).");

        string keystream = "";

        for (int i = 0; i < steps; i++) {
            char newBit = (seed[0] == seed[tap] ? '0' : '1');
            keystream += newBit;

            // Shift left and append the new bit
            seed = seed.Substring(1) + newBit;

            Console.WriteLine($"Step {i + 1}: New Seed = {seed}, Rightmost Bit = {newBit}");
        }

        // Save the keystream to a file
        string fileName = "keystream.txt";
        File.WriteAllText(fileName, keystream);
        Console.WriteLine($"Keystream saved to {fileName}");
    }

    /// Validates a string is a binary string
    static bool IsBinaryString(string str) {
        foreach (char c in str) {
            if (c != '0' && c != '1')
                return false;
        }
        return true;
    }
}