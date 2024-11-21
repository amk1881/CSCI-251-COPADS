
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

// TODO 
// Decrypt longer version wrong 

/*
Main Program functionality to handle prime/odd generation 
*/
class Program {
    
    // Main program that handles input processing 
    static void Main(string[] args) {
        try {
            if (args.Length < 1 ) {
                String msg = "Usage: <option> <other arguments> \n" +
                "option  -  'cipher', 'GenerateKeystream' , 'Encrypt', 'Decrypt', 'MutlipleBits', 'EncryptImage', 'DecryptImage'\n" +
                "other arguments  -  option specific args\n";
                
                throw new ArgumentException(msg);
            }
            

            string option = args[0];
            //check valid option input 
            if (option != "cipher" && option != "GenerateKeystream" && option != "Encrypt" && 
                option != "Decrypt" && option != "MutlipleBits" && option != "EncryptImage" && option != "DecryptImage" ) 
                
                throw new ArgumentException("'option' must be a valid argument.");
                
            switch (option)
            {
                case "cipher":
                    if (args.Length != 3)
                        throw new ArgumentException("Usage for 'cipher': cipher <seed> <tap>\n");
                    string c_seed = args[1];
                    int c_tap = int.Parse(args[2]);
                    Cipher(c_seed, c_tap);
                    break;

                case "GenerateKeystream":
                    if (args.Length != 4)
                        throw new ArgumentException("Usage for 'GenerateKeystream': GenerateKeyStream <seed> <tap> <steps>\n");
                    string ks_seed = args[1];
                    int ks_tap = int.Parse(args[2]);
                    int steps = int.Parse(args[3]);
                    GenerateKeystream(ks_seed, ks_tap, steps);
                    break;

                case "Encrypt":
                    if (args.Length != 2)
                        throw new ArgumentException("Usage for 'Encrypt': Encrypt <plaintext>\n");
                    string plaintext = args[1];
                    Encrypt(plaintext);
                    break;

                case "Decrypt":
                    if (args.Length != 2)
                        throw new ArgumentException("Usage for 'Decrypt': Decrypt <ciphertext>\n");
                    string ciphertext = args[1];
                    Decrypt(ciphertext);
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

        Console.WriteLine($"{seed}-seed");
        Console.WriteLine($"{newSeed}   {newBit}");
    }

    /** Generates a keystream by simulating multiple steps of the LFSR cipher
        takes a seed, tap position, and the number of steps
    */
    static void GenerateKeystream(string seed, int tap, int steps) {
        if (seed.Length <= tap || tap < 0)
            throw new ArgumentException("Tap position must be within the length of the seed.");

        if (steps <= 0)
            throw new ArgumentException("Number of steps must be a positive integer.");

        if (!IsBinaryString(seed))
            throw new ArgumentException("Seed must be a binary string (only 0s and 1s).");

        string currentSeed = seed; 
        string keystream = "";    

        for (int i = 0; i < steps; i++) {
            // Access the tap bit from the end of the seed
            int tapBitIndex = currentSeed.Length - tap ;
            char tapBit = currentSeed[tapBitIndex]; 
            
            // XOR between leftmost bit and tap bit
            char newBit = (currentSeed[0] == tapBit ? '0' : '1');

            // shift left and append the new bit
            currentSeed = currentSeed.Substring(1) + newBit;
            keystream += newBit;
            Console.WriteLine($"{currentSeed}   {newBit}");
        }
        // Print the final keystream
        Console.WriteLine($"The Keystream: {keystream}");

        string fileName = "keystream.txt";
        File.WriteAllText(fileName, keystream);
    }

    /// Validates a string is a binary string
    static bool IsBinaryString(string str) {
        foreach (char c in str) {
            if (c != '0' && c != '1')
                return false;
        }
        return true;
    }

//accept plaintext in bits; perform an XOR operation with the 
//retrieved keystream from the file; and return a set of encrypted bits (ciphertext).
    static void Encrypt(string plaintext) {
    if (!IsBinaryString(plaintext))
        throw new ArgumentException("Plaintext must be a binary string (only 0s and 1s).");

    // Read the keystream from the file
    string keystreamPath = "keystream.txt";
    if (!System.IO.File.Exists(keystreamPath))
        throw new FileNotFoundException("Keystream file not found. Generate it first using 'GenerateKeystream'.");

    string keystream = System.IO.File.ReadAllText(keystreamPath).Trim();

    // Validate the keystream
    if (!IsBinaryString(keystream))
        throw new ArgumentException("Keystream in the file must be a binary string (only 0s and 1s).");

    string ciphertext = "";
    string longer = "";
    string shorter = "";
    if (keystream.Length == plaintext.Length){

        for (int i = 0; i < plaintext.Length; i++) {
            char plaintextBit = plaintext[i];
            char keystreamBit = keystream[i];
            char encryptedBit = (plaintextBit == keystreamBit) ? '0' : '1'; // XOR 
            ciphertext += encryptedBit;
        }
    }
    else {
        if (keystream.Length > plaintext.Length) {
            longer = keystream; 
            shorter = plaintext; 
        }
        else { 
            longer = plaintext; 
            shorter = keystream; 
        }
        int overflowCount = longer.Length - shorter.Length; 
        string overflowBits = longer.Substring(0, overflowCount );
        ciphertext += overflowBits; 
        string remainingBits = longer.Substring(overflowCount );

            // Encrypt the plaintext using XOR
        for (int i = 0; i < shorter.Length; i++) {
            char shorterBit = shorter[i];
            char longerRemainingBit= remainingBits[i];
            char encryptedBit = (shorterBit == longerRemainingBit) ? '0' : '1'; // XOR 
            ciphertext += encryptedBit;
        }
    }


    // Output the ciphertext
    Console.WriteLine($"The ciphertext is: {ciphertext}");
    }

    //generating a plaintext in bits from a given ciphertext (in bits) using the 
    //keystream with the encrypt test cases
    static void Decrypt(string ciphertext)
    {
        // Validate the ciphertext
        if (!IsBinaryString(ciphertext))
            throw new ArgumentException("Ciphertext must be a binary string (only 0s and 1s).");

        // Read the keystream from the file
        string keystreamPath = "keystream.txt";
        if (!System.IO.File.Exists(keystreamPath))
            throw new FileNotFoundException("Keystream file not found. Generate it first using 'GenerateKeystream'.");

        string keystream = System.IO.File.ReadAllText(keystreamPath).Trim();

        // Validate the keystream
        if (!IsBinaryString(keystream))
            throw new ArgumentException("Keystream in the file must be a binary string (only 0s and 1s).");

            
        string plaintext = "";
        string longer = "";
        string shorter = "";
        if (keystream.Length == plaintext.Length){

            for (int i = 0; i < plaintext.Length; i++) {
                char plaintextBit = plaintext[i];
                char keystreamBit = keystream[i];
                char decryptedBit = (plaintextBit == keystreamBit) ? '0' : '1'; // XOR 
                plaintext += decryptedBit;
            }
        }
        else {
            if (keystream.Length > ciphertext.Length) {
                longer = keystream; 
                shorter = ciphertext; 
            }
            else { 
                longer = ciphertext; 
                shorter = keystream; 
            }
            int overflowCount = longer.Length - shorter.Length; 
            string overflowBits = longer.Substring(0, overflowCount );
            ciphertext += overflowBits; 
            string remainingBits = longer.Substring(overflowCount );

                // Encrypt the plaintext using XOR
            for (int i = 0; i < shorter.Length; i++) {
                char shorterBit = shorter[i];
                char longerRemainingBit= remainingBits[i];
                char decryptedBit = (shorterBit == longerRemainingBit) ? '0' : '1'; // XOR 
                plaintext += decryptedBit;
            }
        }


        // Output the plaintext
        Console.WriteLine($"The plaintext is: {plaintext}");
    }

}