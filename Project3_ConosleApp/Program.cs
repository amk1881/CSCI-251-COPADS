
/*
CSCI 251 Project 3
Author: Anna Kurchenko
Date: 11/20/24
*/

using System;
using System.Dynamic;
using System.IO;
using SkiaSharp;

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
                option != "Decrypt" && option != "MultipleBits" && option != "EncryptImage" && option != "DecryptImage" ) 
                
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

                case "MultipleBits":
                    if (args.Length != 5)
                        throw new ArgumentException("Usage for 'MultipleBits': MultipleBits <seed> <tap> <steps> <iteration> \n");
                    string mb_seed = args[1];
                    int mb_tap = int.Parse(args[2]);
                    int mb_steps = int.Parse(args[3]);
                    int iteration = int.Parse(args[4]);
                    MultipleBits(mb_seed, mb_tap, mb_steps, iteration);
                    break;

                case "EncryptImage":
                    if (args.Length != 4)
                        throw new ArgumentException("Usage for 'EncryptImage': EncryptImage <imagefile> <seed> <tap>\n");
                    string imagefile = args[1];
                    string e_seed = args[2];
                    int e_tap = int.Parse(args[3]);
                    EncryptImage(imagefile, e_seed, e_tap);
                    break;

                case "DecryptImage":
                    if (args.Length != 4)
                        throw new ArgumentException("Usage for 'DecryptImage': DecryptImage <imagefile> <seed> <tap>\n");
                    string d_imagefile = args[1];
                    string d_seed = args[2];
                    int d_tap = int.Parse(args[3]);
                    DecryptImage(d_imagefile, d_seed, d_tap);
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
            throw new ArgumentException("Seed must be a binary string");

        // XOR the bit at the tap position with the leftmost bit
        char newBit = (seed[0] == seed[tap] ? '0' : '1');

        // Shift left and append the new bit
        string newSeed = seed.Substring(1) + newBit;

        Console.WriteLine($"{seed} - seed");
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
            throw new ArgumentException("Seed must be a binary string ");

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
        throw new ArgumentException("Plaintext must be a binary string");

    // Read the keystream from the file
    string keystreamPath = "keystream.txt";
    if (!System.IO.File.Exists(keystreamPath))
        throw new FileNotFoundException("Keystream file not found. generate it using 'GenerateKeystream'.");

    string keystream = System.IO.File.ReadAllText(keystreamPath).Trim();

    // Validate the keystream
    if (!IsBinaryString(keystream))
        throw new ArgumentException("Keystream in the file must be a binary string");

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
            throw new ArgumentException("Ciphertext must be a binary string");

        // Read the keystream from the file
        string keystreamPath = "keystream.txt";
        if (!System.IO.File.Exists(keystreamPath))
            throw new FileNotFoundException("Keystream file not found. generate it using 'GenerateKeystream'");

        string keystream = System.IO.File.ReadAllText(keystreamPath).Trim();

        // Validate the keystream
        if (!IsBinaryString(keystream))
            throw new ArgumentException("Keystream in the file must be a binary string");

            
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
            plaintext += overflowBits; 
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

    /**
    This option will accept an initial seed, tap, step - a positive integer 
    (let’s say p) and perform ‘p’ steps of the LFSR cipher simulation. It will also accept iteration- a positive 
    integer (let’s say w). After each iteration i (0 ≤ i <w), it returns a new seed, and accumulated integer 
    value.  
    At each step, it performs the cipher simulation; gets the recent rightmost bit and performs an 
    arithmetic with this bit value. 
    */
     static void MultipleBits(string seed, int tap, int step, int iteration) {
        // Validate inputs
        if (!IsBinaryString(seed))
            throw new ArgumentException("Seed must be a binary string");
        if (tap <= 0)
            throw new ArgumentException("Tap must be a positive integer");
        if (step <= 0)
            throw new ArgumentException("Step must be a positive integer");
        if (iteration <= 0)
            throw new ArgumentException("Iteration must be a positive integer");

        Console.WriteLine($"{seed} - seed");

        for (int i = 0; i < iteration; i++) { // #iteraitons 
            string currentSeed = seed;
            int accumulatedValue = 0;

            // Perform 'step' LFSR simulations
            for (int j = 0; j < step; j++) {
                // get new bit 
                int tapIndex = seed.Length - tap; 
                char tapBit = currentSeed[tapIndex];
                char newBit = (currentSeed[0] == tapBit) ? '0' : '1'; // XOR operation

                currentSeed = currentSeed.Substring(1) + newBit;    // update 

                char rightmostBit = currentSeed[^1];

                accumulatedValue = (accumulatedValue * 2) + (rightmostBit - '0'); // Multiply by 2, add bit value
            }

            Console.WriteLine($"{currentSeed}  {accumulatedValue}");
            // Update seed for the next iteration
            seed = currentSeed;
        }
}

//Given an image with a seed and a tap position , generate a row- encrypted image
    static void EncryptImage(string imagePath, string seed, int tap) {
        // Validate input
        if (!File.Exists(imagePath))
            throw new FileNotFoundException($"Image file not found: {imagePath}");

        if (!IsBinaryString(seed))
            throw new ArgumentException("Seed must be a binary string");

        if (tap <= 0)
            throw new ArgumentException("Tap must be a positive integer");

        // Load  image as a bitmap
        using var originalBitmap = SKBitmap.Decode(imagePath);
        if (originalBitmap == null)
            throw new Exception("Failed to load image.");

        var encryptedBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);
        string currentSeed = seed;

        for (int y = 0; y < originalBitmap.Height; y++) {
            for (int x = 0; x < originalBitmap.Width; x++) {
                SKColor pixelColor = originalBitmap.GetPixel(x, y);  // Get each pixel color

                // Generate random 8-bit integers for R, G, B using the LFSR
                byte randomRed = GenerateRandomByte(ref currentSeed, tap);
                byte randomGreen = GenerateRandomByte(ref currentSeed, tap);
                byte randomBlue = GenerateRandomByte(ref currentSeed, tap);

                // XOR each color component with the corresponding random number
                byte newRed = (byte)(pixelColor.Red ^ randomRed);
                byte newGreen = (byte)(pixelColor.Green ^ randomGreen);
                byte newBlue = (byte)(pixelColor.Blue ^ randomBlue);

                SKColor encryptedColor = new SKColor(newRed, newGreen, newBlue, pixelColor.Alpha);
                encryptedBitmap.SetPixel(x, y, encryptedColor);
            }
        }

        // Save the encrypted image
        string directory = Path.GetDirectoryName(imagePath) ?? ".";
        string originalFileName = Path.GetFileNameWithoutExtension(imagePath);
        string encryptedFileName = $"{originalFileName}ENCRYPTED{Path.GetExtension(imagePath)}";
        string encryptedFilePath = Path.Combine(directory, encryptedFileName);

        using var image = SKImage.FromBitmap(encryptedBitmap);
        using var data = image.Encode();
        File.WriteAllBytes(encryptedFilePath, data.ToArray());

        Console.WriteLine($"Encrypted image saved as: {encryptedFileName}");
    }

    // Generate a random 8-bit unsigned integer using LFSR
    static byte GenerateRandomByte(ref string seed, int tap) {
        byte randomByte = 0;

        // for 8 bits
        for (int i = 0; i < 8; i++) {
            int tapIndex = seed.Length - tap;
            char tapBit = seed[tapIndex];
            char newBit = (seed[^1] == tapBit) ? '0' : '1'; // XOR operation

            seed = seed.Substring(1) + newBit;

            randomByte = (byte)((randomByte << 1) | (newBit - '0'));
        }
        return randomByte;
    }


    // remove the wrod "ENCRYPTED" from the file name
    static string GetRealImageFileName(string imgFile){
        // Extract original file name without the extension
        string imgDir = Path.GetDirectoryName(imgFile) ?? ".";
        string originalFileName = Path.GetFileNameWithoutExtension(imgFile);
        string extension = Path.GetExtension(imgFile);

        // If the filename contains "Encrypted", remove it
        if (originalFileName.EndsWith("Encrypted", StringComparison.OrdinalIgnoreCase))
            originalFileName = originalFileName.Substring(0, originalFileName.Length - "Encrypted".Length);

        // Generate output filename
        string outputFileName = Path.Combine(imgDir, $"{originalFileName}NEW{extension}");

        return outputFileName;

    }

    static void DecryptImage(string imgFile, string seed, int tap) {
        // Validate input
        if (!File.Exists(imgFile))
            throw new FileNotFoundException($"Encrypted image file not found: {imgFile}");

        if (!IsBinaryString(seed))
            throw new ArgumentException("Seed must be a binary string (only 0s and 1s).");

        if (tap <= 0 || tap > seed.Length)
            throw new ArgumentException("Tap must be a positive integer within the seed's bit length.");

        // Load image as a bitmap
        using var encryptedBitmap = SKBitmap.Decode(imgFile);
        if (encryptedBitmap == null)
            throw new Exception("Failed to load encrypted image.");


        var decryptedBitmap = new SKBitmap(encryptedBitmap.Width, encryptedBitmap.Height);
        string currentSeed = seed;

        for (int y = 0; y < encryptedBitmap.Height; y++)  {
            for (int x = 0; x < encryptedBitmap.Width; x++) {
                
                SKColor encryptedColor = encryptedBitmap.GetPixel(x, y); // Get each pixel color

                // Generate random 8-bit integers for R, G, B using the LFSR
                byte randomRed = GenerateRandomByte(ref currentSeed, tap);
                byte randomGreen = GenerateRandomByte(ref currentSeed, tap);
                byte randomBlue = GenerateRandomByte(ref currentSeed, tap);

                // XOR each encrypted color component with the corresponding random number
                byte originalRed = (byte)(encryptedColor.Red ^ randomRed);
                byte originalGreen = (byte)(encryptedColor.Green ^ randomGreen);
                byte originalBlue = (byte)(encryptedColor.Blue ^ randomBlue);

                SKColor originalColor = new SKColor(originalRed, originalGreen, originalBlue, encryptedColor.Alpha);
                decryptedBitmap.SetPixel(x, y, originalColor);
            }
        }

        // get file name without the 'ENCRYPTED' 
        string outputFileName = GetRealImageFileName(imgFile);

        // Save the decrypted image
        using var image = SKImage.FromBitmap(decryptedBitmap);
        using var data = image.Encode();
        File.WriteAllBytes(outputFileName, data.ToArray());

    }

}