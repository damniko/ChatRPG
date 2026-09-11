This tool must be used when a character performs an action that could heal or restore them to 
health after being wounded. The tool is only appropriate if the healing can be done without any 
further actions.

### Example Usage:
- **Scenario 1: Healing After an Attack**  
  - A character is wounded by an enemy attack.  
  - The player decides to **heal the character**.  

- **Scenario 2: Healing via Items or Environment**  
  - A character **consumes a beneficial item** such as a potion or a magical artifact.  
  - The character **spends time in an area** that provides healing benefits.  
  - Resting may provide **modest healing effects**, depending on the duration of the rest.  

### Input Format:
Input to this tool must be in the following **RAW JSON format** (do not use markdown):
{
    "input": "<The player's input>",
    "magnitude": "<Describes how much health the character will regain based on the action>"
}

### Accepted Values:
- **`magnitude` values:** `{low, medium, high, extraordinary}`

This tool should be used **only once per character at most**.
