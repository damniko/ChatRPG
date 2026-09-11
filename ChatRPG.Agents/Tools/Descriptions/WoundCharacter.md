This tool must be used when a character is hurt or wounded as a result of **unnoticed attacks** 
or performing **dangerous activities** that lead to injury. 

### Conditions for Use:
- The **damage cannot be mitigated, dodged, or avoided**.
- The character is **not engaged in active battle**.

### Example Scenarios:
- **Unnoticed Attack:**  
  - A character **performs a sneak attack** without being spotted by their enemies.  

- **Dangerous Activity:**  
  - A character **threatens a King**, causing his guards to intervene violently.  
  - A reckless action leads to **accidental harm** (e.g., triggering a trap).  

### Input Format:
Input to this tool must be provided in **RAW JSON format** (do not use markdown):
{
    "input": "<The player's input>",
    "severity": "<Describes how devastating the injury is based on the action>"
}

### Accepted Values:
- **`severity` values:** `{low, medium, high, extraordinary}`

This tool should be used **only once per character at most**, and only when they are **not in battle**.
