using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;

//Data structure
[DataContract]
public class Book
{
    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "publishYear")]
    public int PublishYear { get; set; }

    [DataMember(Name = "price")]
    public double Price { get; set; }
}

[DataContract]
public class BookStore
{
    [DataMember(Name = "books")]
    public List<Book> Books { get; set; }
}


internal class DataMonitor
{
    private Book[] _books; //A container for all the books
    private int _count; //Number of elements
    private readonly int _capacity;
    private int _start;
    private int _end;
    private readonly object _lock = new(); //Creates a new lock
    public bool EverythingAdded;

    //A constructor for a data monitor
    public DataMonitor(int capacity)
    {
        _capacity = capacity;
        _count = 0;
        _books = new Book[capacity];
    }

    public void AddItem(Book book)
    {
        lock (_lock)
        {
            while (_count >= _capacity) //if its full
            {
                Console.WriteLine("DataMonitor is full!");
                Monitor.Wait(_lock);
            }

            _books[_end] = book;
            _end = _end + 1;
            //Makes the array loop
            if (_end >= _capacity)
            {
                _end = 0;
            }
            _count++;

            Console.WriteLine($"Element has been added to DataMonitor. New element count: {_count}, Element: {book.Name}");
            Monitor.Pulse(_lock);
        }
    }

    public Book RemoveItem()
    {
        lock (_lock)
        {
            while (_count == 0) //if its empty
            {
                if (EverythingAdded)
                    return null;
                Console.WriteLine("DataMonitor is empty!");
                Monitor.Wait(_lock);
            }

            Book book = _books[_start];
            _start = _start + 1;
            //Makes the array loop
            if (_start >= _capacity)
            {
                _start = 0;
            }
            _count--;

            Console.WriteLine($"Element has been taken from DataMonitor. New element count: {_count}, Element: {book.Name}");
            Monitor.Pulse(_lock);
            return book;
        }
    }
}

internal class BookComputeValue
{
    public double CalculatedValue { get; set; }
    public Book OriginalData { get; set; }

    public override string ToString()
    {
        return $"{OriginalData.Name} {OriginalData.PublishYear} {OriginalData.Price} {CalculatedValue}";
    }
}

internal class SortedResultMonitor
{
    private BookComputeValue[] _results; //A container for all results
    private int _count; //Number of elements
    private readonly int _capacity;
    private readonly object _lock = new(); //Creates a new lock

    //A constructor for a result monitor
    public SortedResultMonitor(int capacity)
    {
        _count = 0;
        _capacity = capacity;
        _results = new BookComputeValue[capacity];
    }

    public void AddItemSorted(BookComputeValue result)
    {
        lock (_lock)
        {

            //Add an element in a way that the array is still sorted
            int index = LinearSearch(result);
            ShiftElementsRight(index);
            _results[index] = result;
            _count++;
            Console.WriteLine($"Element has been added to ResultMonitor. New element count: {_count}, Element: {result.OriginalData.Name}");
        }
    }

    //Returns all computed and sorted elements
    public BookComputeValue[] GetItems()
    {
        lock (_lock)
        {
            return _results.Take(_count).ToArray();
        }
    }

    //Finds a fitting stop for the new element
    private int LinearSearch(BookComputeValue result)
    {
        for (int i = 0; i < _count; i++)
        {
            if (result.CalculatedValue <= _results[i].CalculatedValue)
            {
                return i;
            }
        }
        return _count;
    }

    //Shifts elements to make space for the given index
    private void ShiftElementsRight(int startIndex)
    {
        for (int i = _count - 1; i >= startIndex; i--)
        {
            _results[i + 1] = _results[i];
        }
    }
}


internal class Program
{
    public static void Main()
    {
        //Reads data from a file
        int condition = 1; // Change this value to test different cases
        string dataName = "";
        string rezFile = "";
        switch (condition)
        {
            case 1:
                dataName = "IFF-1-4_MildarasA_L1a_dat_1.json";
                rezFile = "IFF-1-4_MildarasA_L1a_rez_1.txt";
                break;

            case 2:
                dataName = "IFF-1-4_MildarasA_L1a_dat_2.json";
                rezFile = "IFF-1-4_MildarasA_L1a_rez_2.txt";
                break;

            case 3:
                dataName = "IFF-1-4_MildarasA_L1a_dat_3.json";
                rezFile = "IFF-1-4_MildarasA_L1a_rez_3.txt";
                break;

            default:
                Console.WriteLine("Invalid option");
                break;
        }

        List<Book> data = ReadDataFromFile(dataName);
        //Creates monitors with the given sizes
        File.Delete(rezFile);
        DataMonitor dataMonitor = new(8);
        SortedResultMonitor resultMonitor = new(64);
        //Starts worker threads
        Thread worker = new(() => WorkerThread(dataMonitor, resultMonitor));
        Thread worker2 = new(() => WorkerThread(dataMonitor, resultMonitor));
        Thread worker3 = new(() => WorkerThread(dataMonitor, resultMonitor));
        Thread worker4 = new(() => WorkerThread(dataMonitor, resultMonitor));
        Thread worker5 = new(() => WorkerThread(dataMonitor, resultMonitor));
        worker.Start();
        worker2.Start();
        worker3.Start();
        worker4.Start();
        worker5.Start();
        //Main thread puts items in the monitor
        foreach (Book book in data)
        {
            //Thread.Sleep(250);
            dataMonitor.AddItem(book);
        }
        //Gives a flag that all data has been given
        dataMonitor.EverythingAdded = true;
        worker.Join();
        worker2.Join();
        worker3.Join();
        worker4.Join();
        worker5.Join();

        // Write results to a txt file
        using StreamWriter file = new StreamWriter(rezFile, append: false);

        file.WriteLine("Given data:");

        if (data.Count == 0)
        {
            file.WriteLine("No elements given.");
        }
        else
        {
            file.WriteLine("--------------------------------------------------------------------");
            file.WriteLine("| {0, -5} | {1, -20} | {2, -15} | {3, -15} |", "No.", "Name", "Publish Year", "Price");
            file.WriteLine("--------------------------------------------------------------------");
            int counter = 1;
            foreach (Book book in data)
            {
                file.WriteLine("| {0, 5} | {1, -20} | {2, 15} | {3, 15} |", counter++, book.Name, book.PublishYear, book.Price);
            }
            file.WriteLine("--------------------------------------------------------------------");

        }
        file.WriteLine();
        file.WriteLine("Filtered results:");


        BookComputeValue[] results = resultMonitor.GetItems();
        if (results.Length == 0)
        {
            file.WriteLine("No elements found.");
        }
        else
        {
            file.WriteLine("---------------------------------------------------------------------------------------");
            file.WriteLine("| {0, -5} | {1, -20} | {2, -15} | {3, -15} | {4, -16} |", "No.", "Name", "Publish Year", "Price", "Calculated Value");
            file.WriteLine("---------------------------------------------------------------------------------------");
            int counter = 1;
            foreach (BookComputeValue result in results)
            {
                file.WriteLine("| {0, 5} | {1, -20} | {2, 15} | {3, 15} | {4, 16} |", counter++, result.OriginalData.Name, result.OriginalData.PublishYear, result.OriginalData.Price, result.CalculatedValue);
            }
            file.WriteLine("---------------------------------------------------------------------------------------");
        }
        file.Close();
    }

    private static void WorkerThread(DataMonitor dataMonitor, SortedResultMonitor resultMonitor)
    {
        while (true)
        {
            Book book = dataMonitor.RemoveItem(); //lock
            if (book == null)
                break; //nothing is left
            double calculatedValue = Calculation(book.Price, book.PublishYear); //do some calculation
            //If the answer is high enough add the result
            //Thread.Sleep(500);
            if (calculatedValue < 10000)
            {
                Console.WriteLine($"Element has failed the criteria. Result: {calculatedValue}, Element: {book.Name}");
                continue;
            }
            BookComputeValue result = new() { OriginalData = book, CalculatedValue = calculatedValue };
            Console.WriteLine($"Element has passed the criteria. Result: {calculatedValue}, Element: {book.Name}");
            resultMonitor.AddItemSorted(result);
        }
    }
    private static double Calculation(double price, int publishYear)
    {
        double result = 0;
        for (int i = 0; i < 20000; i++) //Ensures some computational effort
        {
            for (int j = 0; j < price + price; j++) //Ensure some result value increase
            {
                result++;
            }
        }
        return result * 0.01;
    }
    //Deserializes all data from a json file
    private static List<Book> ReadDataFromFile(string filename)
    {
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BookStore));
        using FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
        if (stream.Length == 0)
        {
            Console.WriteLine("File is empty");
            return new List<Book>();
        }
        BookStore bookStore = (BookStore)serializer.ReadObject(stream);
        List<Book> booksList = bookStore.Books;
        return booksList;
    }
}


