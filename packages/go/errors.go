package axene

import "fmt"

// Error is raised for any non-2xx API response, or for a transport failure
// that survives all retries. Inspect Status and Code to branch on specific
// failures (for example a 422 with code "invalid").
type Error struct {
	// Status is the HTTP status code. 0 indicates a transport or network
	// failure where no response was received.
	Status int
	// Code is the machine-readable error code from the API body, when present.
	Code string
	// Message is a human-readable description of the failure.
	Message string
}

// Error implements the error interface.
func (e *Error) Error() string {
	if e.Code != "" {
		return fmt.Sprintf("axene: %s (status %d, code %s)", e.Message, e.Status, e.Code)
	}
	return fmt.Sprintf("axene: %s (status %d)", e.Message, e.Status)
}
