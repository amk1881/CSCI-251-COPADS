// See https://aka.ms/new-console-template for more information

/*
CSCI 251 Project 2
Author: Anna Kurchenko
Date: 10/28/24
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
class Program
{

    
    static void Main(string[] args)
    {
        try {
            if (args.Length < 2 ) {
                String msg = "Usage: <bits> <option> <count> \n" +
                "bits - the  number  of  bits  of  the  number  to  be  generated,  this  must be  a multiple  of  8,  and  at  least  32  bits\n" +
                "option  -  'odd'  or  'prime'  (the  type  of  numbers  to  be  generated)\n" +
                "count  -  the  count  of  numbers  to  generate,  defaults  to  1\n";
                
                throw new ArgumentException(msg);
            }
            
        int bits ;
        if (!int.TryParse(args[0], out bits) || bits > 32 || bits % 8 != 0)
        {
            throw new ArgumentException("'bits' must be a multiple of 8 and at least 32.");
        }

        string option = args[1];
        if (option != "odd" && option != "prime")
        {
            throw new ArgumentException("'option' must be either 'odd' or 'prime'.");
        }

        int count = 1; // Default count
        if (args.Length == 3)
        {
            if (!int.TryParse(args[2], out count) || count < 1)
            {
                throw new ArgumentException("'count' must be a positive integer.");
            }
        }

        Console.WriteLine("Generating " + count + " " + option + " numbers with " + bits + " bits...");

            if (option == "prime")
            {
                GeneratePrimes(bits, count);
            }
            else
            {
                GenerateOddFactors(bits, count);
            }        
        }
        
        catch (ArgumentException ex) { 
            Console.WriteLine(ex.Message); 
        }
    }

    // Method to generate prime numbers
    static void GeneratePrimes(int bits, int count)
    {
        var primes = new ConcurrentBag<BigInteger>();
        Parallel.For(0, count, i =>
        {
            BigInteger number;
            do
            {
                number = GenerateRandomBigInteger(bits);
            } while (!IsProbablyPrime(number));

            primes.Add(number);
            Console.WriteLine("Prime: " + number);
        });
    }

    // Method to generate odd numbers and their factors
    static void GenerateOddFactors(int bits, int count)
    {
        Parallel.For(0, count, i =>
        {
            BigInteger number = GenerateRandomBigInteger(bits) | 1; // Ensure number is odd
            var factors = FindFactors(number);
            Console.WriteLine($"Odd Number: {number}, Factors: {string.Join(", ", factors)}");
        });
    }

    // Helper method to generate a random BigInteger
    static BigInteger GenerateRandomBigInteger(int bits)
    {
        byte[] bytes = new byte[bits / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return new BigInteger(bytes, isUnsigned: true);
    }

    // Extension method to check if a BigInteger is probably prime using Miller-Rabin
    static bool IsProbablyPrime(BigInteger value, int k = 10)
    {
        if (value < 2) return false;
        if (value == 2 || value == 3) return true;
        if (value % 2 == 0) return false;

        BigInteger d = value - 1;
        int s = 0;
        while (d % 2 == 0)
        {
            d /= 2;
            s += 1;
        }

        for (int i = 0; i < k; i++)
        {
            BigInteger a = GenerateRandomBigInteger(GetBitLength(value) - 1) % (value - 2) + 2;
            BigInteger x = BigInteger.ModPow(a, d, value);
            if (x == 1 || x == value - 1)
                continue;

            bool composite = true;
            for (int r = 1; r < s; r++)
            {
                x = BigInteger.ModPow(x, 2, value);
                if (x == value - 1)
                {
                    composite = false;
                    break;
                }
                if (x == 1) return false;
            }
            if (composite) return false;
        }
        return true;
    }


    public static BigInteger Sqrt(BigInteger value)
    {
        if (value < 0) throw new ArgumentException("Negative argument.");
        if (value == 0) return 0;

        BigInteger n = (value >> 1) + 1; // Initial guess (value / 2 + 1)
        BigInteger n1 = (n + (value / n)) >> 1;

        while (n1 < n)
        {
            n = n1;
            n1 = (n + (value / n)) >> 1;
        }

        return n;
    }
    

    // Method to find factors of an odd number
    static List<BigInteger> FindFactors(BigInteger number)
    {
        List<BigInteger> factors = new List<BigInteger>();
        BigInteger sqrt = Sqrt(number);

        for (BigInteger i = 1; i <= sqrt; i += 2) // Only check odd numbers
        {
            if (number % i == 0)
            {
                factors.Add(i);
                if (i != number / i)
                {
                    factors.Add(number / i);
                }
            }
        }
        factors.Sort();
        return factors;
    }


    // Extension method to get bit length of BigInteger for generating random numbers with appropriate size
    public static int GetBitLength(BigInteger value)
    {
        byte[] bytes = value.ToByteArray();
        int bitLength = (bytes.Length - 1) * 8;
        int lastByte = bytes[^1];

        while (lastByte > 0)
        {
            bitLength++;
            lastByte >>= 1;
        }
        return bitLength;
    }


}