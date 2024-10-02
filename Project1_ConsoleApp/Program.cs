// See https://aka.ms/new-console-template for more information

/*
CSCI 251 Project 1 
Author: Anna Kurchenko
Date: 10/1/24
*/

using System.Diagnostics;
using System.Collections.Concurrent;

/*
To Do: 
comments
Make sure Optimal Solution
txt file 
validate on alt OS
*/

class Program
{
    struct Stats
    {
        public int FolderCount;
        public int FileCount;
        public int ImageFileCount;
        public long TotalSize;
        public long ImageFileSize;
    
        public void Add(Stats other)
        {
            FolderCount += other.FolderCount;
            FileCount += other.FileCount;
            ImageFileCount += other.ImageFileCount;
            TotalSize += other.TotalSize;
            ImageFileSize += other.ImageFileSize;
        }
    
    }

    static void Main(string[] args)
    {
        try {
            if (args.Length != 2 || (args[0] != "-s" && args[0] != "-d" && args[0] != "-b" ) || !Directory.Exists(args[1])) {
                String msg = "Usage: du [-s] [-d] [-b] <path>\n" +
                "Summarize  disk  usage  of  the  set  of  FILES,  recursively  for  directories.\n" +
                "You  MUST  specify  one  of  the  parameters,  -s,  -d,  or  -b\n" +
                "-s\tRun  in  single  threaded  mode\n" + 
                "-d\tRun  in  parallel  mode  (uses  all  available  processors)\n" +
                "-b\tRun  in  both  parallel  and  single  threaded  mode.\n" + 
                "\tRuns  parallel  followed  by  sequential  mode"    ;
                
                throw new ArgumentException(msg);
            }
            
            char mode = args[0][1];
            string path =  args[1];

            if (mode == 's' || mode == 'b') {
                var stopwatch = Stopwatch.StartNew();

                Stats sequentialResults = sequential(new DirectoryInfo(path));
                stopwatch.Stop();

                Console.WriteLine($"Sequential Calculated in: {stopwatch.Elapsed.TotalSeconds}s");
                reportStatistics(sequentialResults);
            }

            if (mode == 'd' || mode == 'b') {
                var stopwatch = Stopwatch.StartNew();
                Stats parallelResults = parallel(new DirectoryInfo(path));
                stopwatch.Stop();

                Console.WriteLine($"Parallel Calculated in: {stopwatch.Elapsed.TotalSeconds}s");
                reportStatistics(parallelResults);
            }
        }
        catch (ArgumentException ex) { 
            Console.WriteLine(ex.Message); 
        }
    }


    /* 
    Handles printing results of sequential/parallel file searches
    */
    static void reportStatistics(Stats stats){
        Console.WriteLine($"{ stats.FolderCount} folders, {stats.FileCount } files, {stats.TotalSize} bytes");
        
        if (stats.ImageFileCount > 0)
            Console.WriteLine($"{stats.ImageFileCount } image files, {stats.ImageFileSize} bytes\n");
        else
            Console.WriteLine("No image files found in the directory\n");
    }

    /*
    Handles sequential search 
    */
    static Stats sequential(DirectoryInfo directory) {
        Stats stats = new Stats();
        sequentialDirSearch(directory, ref stats);
        return stats;
    }

        /*
        Recursive function for sequential search
        For each directory, go through and count files/folders
        Do for each subdirectory 
        */
        static void sequentialDirSearch(DirectoryInfo directory, ref Stats stats) {
        try {
            stats.FolderCount++;
            
            foreach (var file in directory.GetFiles()) {
                stats.FileCount++;
                stats.TotalSize += file.Length;

                if (isImageFile(file)) {
                    stats.ImageFileCount++;
                    stats.ImageFileSize += file.Length;
                }
            }

            foreach (var subdir in directory.GetDirectories()) {
                sequentialDirSearch(subdir, ref stats);
            }
        }
        //skip any unathorized directories 
        catch (UnauthorizedAccessException)
        {
            //Console.Write("caught unauth");
        }
    }

    /*
    Handles Parallel search, each thread adds its search results to the bag 
    Thread can only seaerch if it has a lock 
    Directories are searched through via queue 
    */
    static Stats parallel(DirectoryInfo directory) {
        ConcurrentBag<Stats> results = new ConcurrentBag<Stats>();
        
        // Queue for directories
        var directories = new ConcurrentQueue<DirectoryInfo>();
        directories.Enqueue(directory);

        Parallel.For(0, Environment.ProcessorCount, _ => {
            while (directories.TryDequeue(out DirectoryInfo currentDirectory)) {
                try {
                    Stats localStats = new Stats();
                    sequentialDirSearch(currentDirectory, ref localStats);
                    results.Add(localStats);
                }
                catch (UnauthorizedAccessException) {
                    //Console.Write("caught unauth");
                    continue;
                }
            }
        });

        Stats allStats = new Stats();
        foreach (var stat in results)
            allStats.Add(stat);

        return allStats;
    }

    static bool isImageFile(FileInfo file) {
        string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };
        return imageExtensions.Contains(file.Extension.ToLower());
    }
}