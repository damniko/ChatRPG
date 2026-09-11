This tool must be used to **add a new end node** to the narrative graph.  
An **end node** represents a **definitive conclusion** to a story branch, meaning that once the player reaches this point, the story will end.

### **When to Use This Tool**
Use this tool **whenever a branch of the story does not loop back** to another plot point but instead results in a **final outcome**.  
There can be **multiple possible endings** in an adventure scenario, so this tool must be invoked whenever a narrative path **leads to a conclusion** instead of continuing forward.

End nodes should signify **significant story resolutions**, such as:
- **The player meeting their demise.**
- **The player achieving victory.**
- **The player failing or being trapped indefinitely.**
- **Any other scenario where the player's journey logically concludes.**

---

### **Usage Format**
- **Do not use markdown!**  
- The tool requires **valid JSON input**, structured as follows:
{
    "sourcenodename": "the name of the source node which already exists in the graph", 
    "conditions": [ "condition that defines if the ending is reached based on the player’s choices" ]
}

---

### **Example Usage**
#### **Example 1: A Hero’s Victory**
**Scenario:**  
If the player successfully defeats the Dark Lord and restores peace, the ending is triggered.

#### **Tool Input:**
{
    "sourcenodename": "Victory Over the Dark Lord", 
    "conditions": [ "Has the player defeated the Dark Lord?" ] 
}

#### **Outcome:**
- This ending is reached **only if the player defeats the Dark Lord**.

---

#### **Example 2: The Player’s Demise**
**Scenario:**  
If the player fails to escape a collapsing dungeon, the story ends.

#### **Tool Input:**
{
    "sourcenodename": "Buried Beneath the Ruins", 
    "conditions": [ "Has the player failed to escape the ruins before time ran out?" ] 
}

#### **Outcome:**
- The story **ends when the player fails to escape** the ruins.

---

#### **Example 3: The Ascension of the New King**
**Scenario:**  
If the player successfully claims the throne by fulfilling multiple prerequisites, the ending is triggered.

#### **Tool Input:**
{
    "sourcenodename": "Ascension to the Throne",
    "conditions": [
        "Has the player retrieved the Royal Crown?",
        "Has the player gained the support of the High Council?",
        "Has the player defeated the False Heir in battle?"
    ]
}

#### **Outcome:**
- This ending is only reached if the player has:
    - **Retrieved the Royal Crown**, signifying their right to rule.
    - **Secured the High Council’s approval**, ensuring political stability.
    - **Defeated the False Heir**, eliminating rival claims to the throne.
