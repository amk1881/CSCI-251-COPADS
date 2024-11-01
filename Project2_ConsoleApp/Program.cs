// See https://aka.ms/new-console-template for more information

/*
CSCI 251 Project 2
Author: Anna Kurchenko
Date: 10/28/24
*/

using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Security.Cryptography;
using System.Diagnostics;


/*
TODO: 
print counts in order 

*/

/*
Main Program functionality to handle prime/odd generation 
*/
class Program {
    
    // Main program that handles input processing 
    static void Main(string[] args) {
        try {
            if (args.Length < 2 ) {
                String msg = "Usage: <bits> <option> <count> \n" +
                "bits - the  number  of  bits  of  the  number  to  be  generated,  this  must be  a multiple  of  8,  and  at  least  32  bits\n" +
                "option  -  'odd'  or  'prime'  (the  type  of  numbers  to  be  generated)\n" +
                "count  -  the  count  of  numbers  to  generate,  defaults  to  1\n";
                
                throw new ArgumentException(msg);
            }
            
        int bits ;
        if (!int.TryParse(args[0], out bits) || bits < 32 || bits % 8 != 0) 
            throw new ArgumentException("'bits' must be a multiple of 8 and at least 32.");

        string option = args[1];
        if (option != "odd" && option != "prime") 
            throw new ArgumentException("'option' must be either 'odd' or 'prime'.");

        int count = 1; // Default count
        if (args.Length == 3) {
            if (!int.TryParse(args[2], out count) || count < 1) 
                throw new ArgumentException("'count' must be a positive integer.");
        }

            if (option == "prime") {
                Console.Write("BitLength: "+ bits + " bits");
                GeneratePrimes(bits, count);
            }

            else {
                Console.Write("BitLength: "+ bits + " bits\n");
                GenerateOddNumber(bits, count);
            }
        }
        
        catch (ArgumentException ex) { 
            Console.WriteLine(ex.Message); 
        }
    }
    
    // Handles generation of prime numbers

    static void GeneratePrimes(int bits, int count) {
        string[] results = new string[count];
        var stopwatch = Stopwatch.StartNew();

        Parallel.ForEach(Enumerable.Range(0, count), i => {
            BigInteger num;
            using (var rng = RandomNumberGenerator.Create()) {
                do 
                    num = Extensions.GenerateRandomBigInteger(bits);
                while (!num.IsProbablyPrime());
            }
            results[i] = $"\n{i + 1}: {num}";
        });

        stopwatch.Stop();

        foreach (var result in results)
            Console.WriteLine(result);

        Console.WriteLine($"Time to Generate: {stopwatch.Elapsed}\n");
    }

    // Generate odd numbers and get their factors 
    static void GenerateOddNumber(int bits, int count)  {
        var stopwatch = Stopwatch.StartNew(); 
        string[] results = new string[count]; 

        Parallel.For(0, count, i =>  {
            
            //  makes sure number is odd
            BigInteger number = Extensions.GenerateRandomBigInteger(bits) | 1; 
            var factors = GetFactors(number);
            
            results[i] = $"{i + 1}: {number}\nNumber of factors: {factors.Count}";

        });

        stopwatch.Stop(); 

        foreach (var result in results) 
            Console.WriteLine(result);

        Console.WriteLine($"Time to Generate: {stopwatch.Elapsed}\n");

    }



    // Find all factors of a number
    static List<BigInteger> GetFactors(BigInteger number)
    {
        List<BigInteger> factors = new List<BigInteger>();
        BigInteger sqrt = Extensions.Sqrt(number);

        for (BigInteger i = 1; i <= sqrt; i += 2) {
            if (number % i == 0) { 
                factors.Add(i);

                if (i != number / i)
                    factors.Add(number / i);
            }
        }
        factors.Sort();
        return factors;
    }
}

// Extension class to contain Miller-Rabin algorithm for getting primes and helper functions
public static class Extensions {

    //Get the length of a btit 
    public static int GetBitLength(this BigInteger value) {
        byte[] bytes = value.ToByteArray();
        int bitLength = (bytes.Length - 1) * 8;
        int lastByte = bytes[^1];

        while (lastByte > 0) {
            bitLength++;
            lastByte >>= 1;
        }
        return bitLength;
    }


    // Check if a BigInteger is probably prime using Miller-Rabin
    public static bool IsProbablyPrime(this BigInteger value, int k = 10) {
        if (value < 2) 
            return false;

        if (value == 2 || value == 3) 
            return true;

        if (value % 2 == 0) 
            return false;

        BigInteger d = value - 1;
        int s = 0;

        while (d % 2 == 0) {
            d /= 2;
            s += 1;
        }

        for (int i = 0; i < k; i++) {
            BigInteger a = GenerateRandomBigInteger((int) value.GetBitLength() - 1) % (value - 2) + 2;
            BigInteger x = BigInteger.ModPow(a, d, value);

            if (x == 1 || x == value - 1)
                continue;

            bool composite = true;

            for (int r = 1; r < s; r++) {
                x = BigInteger.ModPow(x, 2, value);
                if (x == value - 1) {
                    composite = false;
                    break;
                }
                if (x == 1)
                    return false;
            }
            if (composite)
                return false;
        }
        return true;
    }

    
    // Helper method to generate a random BigInteger
    public static BigInteger GenerateRandomBigInteger(int bits) {
        byte[] bytes = new byte[bits / 8];
        using (var random_num_gen = RandomNumberGenerator.Create())
            random_num_gen.GetBytes(bytes);
        
        return new BigInteger(bytes, isUnsigned: true);
    }


    // calculate the integer square root of a BigInteger
    public static BigInteger Sqrt(BigInteger value) {
        if (value < 0) 
            throw new ArgumentException("Negative argument.");

        if (value == 0) 
            return 0;

        BigInteger n = (value >> 1) + 1; // Initial guess (value / 2 + 1)
        BigInteger n1 = (n + (value / n)) >> 1;

        while (n1 < n) {
            n = n1;
            n1 = (n + (value / n)) >> 1;
        }

        return n;
    }
}
