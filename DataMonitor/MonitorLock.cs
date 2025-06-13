using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

public class MonitorTask
{
    public string row;
    public int size;
    public readonly object _lock = new();
    public bool isDone;
    public int currentVowelsInARow = 0;

    public MonitorTask()
    {
        row = "*";
        size = 0;
        isDone = false;
    }

    public void addVowel(char vowel, int count)
    {
        lock (_lock)
        {
            row = row + vowel;
            size++;
            currentVowelsInARow++;
            if(count >= 14)
            {
                isDone = true;
            }
            Monitor.PulseAll(_lock);
        }
    }

    public void addConsonant(char consonant, int count)
    {
        lock (_lock)
        {

            while (currentVowelsInARow < 3)
            {
                if (isDone)
                {
                    return;
                }
                else
                {
                    Monitor.Wait(_lock);
                }
                if (isDone)
                {
                    return;
                }
            }
            row = row + consonant;
            size++;
            currentVowelsInARow = 0;
            if (count >= 14)
            {
                isDone = true;
            }
            Monitor.PulseAll(_lock);
        }
    }
}

public class Program
{
    public static void Main()
    {
        MonitorTask dataMonitor = new();
        Thread processA = new(() => TaskUtils.WorkerVowel(dataMonitor, 'A'));
        Thread processB = new(() => TaskUtils.WorkerConsonant(dataMonitor, 'B'));
        Thread processC = new(() => TaskUtils.WorkerConsonant(dataMonitor, 'C'));
        processA.Start();
        processB.Start();
        processC.Start();
        while (!dataMonitor.isDone)
        {
            Console.WriteLine(dataMonitor.row);
            Thread.Sleep(25);
        }
        processA.Join();
        processB.Join();
        processC.Join();

        Console.WriteLine("Final: " + dataMonitor.row);

/*      using (StreamReader rd = new StreamReader("Filename", Encoding.GetEncoding(1257)))
        {
            List<int> list = new List<int>();
            string line;
            while ((line = rd.ReadLine()) != null)
            {
                var values = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                DateTime date = DateTime.Parse(values[0]);
                string surname = values[1];
                string name = values[2];
                int productId = int.Parse(values[3]);
                int productCount = int.Parse(values[4]);
                //Worker work = new Worker(date, surname, name, productId, productCount);
                //list.Add(work);
            }
        }*/
    }
}

public class TaskUtils
{
    public static void WorkerVowel(MonitorTask dataMonitor, char symbol) 
    {
        int count = 0;
        while (true)
        {
            Thread.Sleep(50);
            dataMonitor.addVowel(symbol, count);
            if (dataMonitor.isDone)
                break;
            count++;
        }
    }

    public static void WorkerConsonant(MonitorTask dataMonitor, char symbol)
    {
        int count = 0;
        while (true)
        {
            Thread.Sleep(50);
            dataMonitor.addConsonant(symbol, count);
            if (dataMonitor.isDone)
                break;
            count++;
        }
    }
}