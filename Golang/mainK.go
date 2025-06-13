package main

import (
	"fmt"
	"sort"
)

var currentSec int = 0
var currentFir int = 0

func sender(sendChan chan<- int, start, end int, done chan<- struct{}) {
	for i := start; i <= end; i++ {
		fmt.Println("Sending:", i)
		sendChan <- i
		if i > 10 {
			currentSec++
		} else {
			currentFir++
		}
	}
	if currentFir == 10 && currentSec == 10 {
		close(sendChan)
	}
	close(done)
}

func receiver(sendChan <-chan int, evenChan, oddChan chan<- int) {
	for num := range sendChan {
		if num%2 == 0 {
			fmt.Println("Received even:", num)
			evenChan <- num
		} else {
			fmt.Println("Received odd:", num)
			oddChan <- num
		}
	}
	close(evenChan)
	close(oddChan)
}

func printer(receiveChan <-chan int, done chan<- struct{}) {
	var numbers []int
	for num := range receiveChan {
		numbers = append(numbers, num)
	}

	// Print the contents of the array
	sort.Slice(numbers, func(i, j int) bool {
		return numbers[i] < numbers[j]
	})
	fmt.Println(numbers)
	close(done)
}

func main() {

	sendChan := make(chan int)
	evenChan := make(chan int)
	oddChan := make(chan int)

	// Create two printer goroutines, one for even and one for odd numbers
	doneEven := make(chan struct{})
	doneOdd := make(chan struct{})
	done := make(chan struct{})

	go printer(evenChan, doneEven)
	go printer(oddChan, doneOdd)

	// Create sender and receiver goroutines
	go sender(sendChan, 1, 10, done)
	go sender(sendChan, 11, 20, done)
	go receiver(sendChan, evenChan, oddChan)

	// Wait for all sender and receiver goroutines to finish
	<-doneEven
	<-doneOdd
	<-done
}
