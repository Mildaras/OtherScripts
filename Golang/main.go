package main

import (
	"encoding/json"
	"fmt"
	"log"
	"os"
	"text/tabwriter"
	"time"
)

// Define the structures with JSON tags
type Book struct {
	Name        string  `json:"name"`
	PublishYear int     `json:"publishYear"`
	Price       float64 `json:"price"`
}

// Define A BookStore with JSON tag
type BookStore struct {
	Books []Book `json:"books"`
}

// Define result
type BookComputeValue struct {
	originalData    Book
	calculatedValue float64
}

// Read data from a JSON file and make a BookStore
func ReadDataFromFile(filename string) (*BookStore, error) {
	byteValue, err := os.ReadFile(filename)
	if err != nil {
		return nil, err
	}
	var bookStore BookStore
	err = json.Unmarshal(byteValue, &bookStore)
	if err != nil {
		return nil, err
	}
	return &bookStore, nil
}

func DataThread(dataChanIn <-chan Book, dataReqChan <-chan int, dataOutChan chan<- Book, size int, doneChan chan<- struct{}) {
	storage := make([]Book, 0, size)
	dataInIsDone := false

	for {
		// Fill up storage until it's full
		for !dataInIsDone && len(storage) < size {
			select {
			case book, ok := <-dataChanIn:
				if !ok {
					dataInIsDone = true
				} else {
					storage = append(storage, book)
					fmt.Println("Putting a new element to data thread:", book.Name, "| New count:", len(storage))
				}
			default:
			}
		}

		// Start taking elements until storage is empty
		for len(storage) > 0 {
			select {
			case <-dataReqChan:
				dataOutChan <- storage[len(storage)-1]
				fmt.Println("Taking an element from data thread:", storage[len(storage)-1].Name, "| New count:", len(storage)-1)
				storage = storage[:len(storage)-1]
			default:
			}
		}

		// Check if both data input is done and storage is empty
		if dataInIsDone && len(storage) == 0 {
			close(dataOutChan)
			close(doneChan)
			return
		}
	}
}




// Calculations for worker thread
func Calculation(price float64, publishYear int) float64 {
	var result float64 = 0
	for i := 0; i < 20000; i++ { // Ensures some computational effort
		for j := 0; j < int(price)+publishYear/10; j++ { // Ensure some result value increase
			result++
		}
	}
	return result * 0.01
}

// WorkerThread
func WorkerThread(dataReqChan chan<- int, dataOutChan <-chan Book, resultChan chan<- BookComputeValue, doneChan <-chan struct{}) {
	for {
		select {
		case book, ok := <-dataOutChan:
			if !ok {
				return
			}
			calculatedValue := Calculation(book.Price, int(book.PublishYear))
			result := BookComputeValue{
				originalData:    book,
				calculatedValue: calculatedValue,
			}
			if result.calculatedValue > 45000 {
				fmt.Println("Element has passed the criteria, element:", result.originalData.Name, "| value:", result.calculatedValue)
				resultChan <- result
			} else {
				fmt.Println("Element has failed the criteria, element:", result.originalData.Name, "| value:", result.calculatedValue)
			}
		case dataReqChan <- 1:
		case <-doneChan:
			close(resultChan)
			return
		}
	}
}

// Find the spot at which to add the new element
func LinearSearch(results []BookComputeValue, result BookComputeValue) int {
	for i, item := range results {
		if result.calculatedValue <= item.calculatedValue {
			return i
		}
	}
	return len(results)
}

// Shift elements to the right
func ShiftElementsRight(results []BookComputeValue, startIndex int) []BookComputeValue {
	results = append(results, BookComputeValue{})                                // Make space for the new element
	copy(results[startIndex+1:], results[startIndex:])                            // Shift
	return results
}

// ResultThread
func ResultThread(resultChan <-chan BookComputeValue, mainChan chan<- []BookComputeValue, doneChan chan<- struct{}) {
	var results []BookComputeValue
	for result := range resultChan {
		index := LinearSearch(results, result)
		results = ShiftElementsRight(results, index)
		results[index] = result
		fmt.Println("New element added to results:", result.originalData.Name)
	}
	mainChan <- results
	//close(mainChan) // Close mainChan after processing all results
	//close(doneChan)
}


func main() {
	bookStore, err := ReadDataFromFile("IFF-1-4_MildarasA_L1a_dat_3.json")
	rezFile := "IFF-1-4_MildarasA_L1a_rez_3.txt"

	if err != nil {
		log.Fatal(err)
	}

	var dataThreadSize = 8
	var workerThreadCount = 5

	bookChan := make(chan Book)
	dataReqChan := make(chan int)
	dataGiveChan := make(chan Book)
	resultChan := make(chan BookComputeValue)
	mainChan := make(chan []BookComputeValue)
	doneChan := make(chan struct{})

	go DataThread(bookChan, dataReqChan, dataGiveChan, dataThreadSize, doneChan)

	for i := 0; i < workerThreadCount; i++ {
		go WorkerThread(dataReqChan, dataGiveChan, resultChan, doneChan)
	}

	go ResultThread(resultChan, mainChan, doneChan)

	for _, b := range bookStore.Books {
		time.Sleep(35 * time.Millisecond)
		bookChan <- b
	}
	close(bookChan)

	<-doneChan
	close(dataReqChan)
	<-doneChan
	close(resultChan)
	results := <-mainChan
	close(mainChan)

	fmt.Println("Processed Books:")
	for i, book := range results {
		fmt.Printf("%d: %+v\n", i+1, book)
	}

	outputFile, err := os.Create(rezFile)
	if err != nil {
		log.Fatal("Cannot create file", err)
	}
	defer outputFile.Close()
	writer := tabwriter.NewWriter(outputFile, 0, 0, 2, ' ', 0)

	// Writing given data
	_, _ = fmt.Fprintln(writer, "Given data:")
	if len(bookStore.Books) == 0 {
		_, _ = fmt.Fprintln(writer, "No elements given.")
	} else {
		_, _ = fmt.Fprintln(writer, "--------------------------------------------------------------------")
		_, _ = fmt.Fprintf(writer, "| %-5s | %-20s | %-15s | %-15s |\n", "No.", "Name", "Publish Year", "Price")
		_, _ = fmt.Fprintln(writer, "--------------------------------------------------------------------")
		for i, book := range bookStore.Books {
			_, _ = fmt.Fprintf(writer, "| %5d | %-20s | %15d | %15.2f |\n", i+1, book.Name, book.PublishYear, book.Price)
		}
		_, _ = fmt.Fprintln(writer, "--------------------------------------------------------------------")
	}

	// Writing filtered results
	_, _ = fmt.Fprintln(writer, "\nFiltered results:")
	if len(results) == 0 {
		_, _ = fmt.Fprintln(writer, "No elements found.")
	} else {
		_, _ = fmt.Fprintln(writer, "---------------------------------------------------------------------------------------")
		_, _ = fmt.Fprintf(writer, "| %-5s | %-20s | %-15s | %-15s | %-16s |\n", "No.", "Name", "Publish Year", "Price", "Calculated Value")
		_, _ = fmt.Fprintln(writer, "---------------------------------------------------------------------------------------")
		for i, result := range results {
			_, _ = fmt.Fprintf(writer, "| %5d | %-20s | %15d | %15.2f | %16.0f |\n", i+1, result.originalData.Name, result.originalData.PublishYear, result.originalData.Price, result.calculatedValue)
		}
		_, _ = fmt.Fprintln(writer, "---------------------------------------------------------------------------------------")
	}

	err = writer.Flush()
	if err != nil {
		log.Fatal("Error writing to file:", err)
	}
}
