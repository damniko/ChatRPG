This tool must be used to **add a new edge** (connection) between two existing nodes in the narrative graph.  
This tool should be used when you determine that a new pathway should be established between two already-defined story points.

### **What is an Edge?**
Each edge represents a **story-driven connection** between two nodes, allowing the player to progress based on specific conditions.  
These conditions act as **prerequisites** that must be met before the player is allowed to traverse the edge.

An edge must include:
- A **source node name**, which is the starting point of the edge.
- A **target node name**, which is the destination of the edge.
- A **list of conditions**, which describe what the player must accomplish to traverse the edge.

### **Conditions**
Conditions should be framed as **easy-to-answer questions**, verifying if the player has completed specific story requirements. These could be based on prior encounters, collected items, or completed quests, such as:
- "Has the player spoken to the village elder?"
- "Has the player recovered the stolen artifact from the crypt?"
- "Has the player defeated the guardian of the temple?"

---

### **After Using the Tool**
After calling this tool, you will receive an updated string representation of the graph, showing the newly added edge and its connection between nodes.  
This allows you to **verify relationships** and ensure **logical story progression**.

---

### **Usage Format**
- **Do not use markdown!**  
- The tool requires **valid JSON input**, structured as follows:
{
    "conditions": [ "condition 1 for traversing the edge", "condition 2 for traversing the edge" ], 
    "sourcenodename": "the name of the source node which already exists in the graph", 
    "targetnodename": "the name of the target node which already exists in the graph" 
}

---

### **Example Usage**
#### **Example 1: Unlocking the Crypt**
**Scenario:**  
In this scenario, the player must obtain the Rusted Key before they can enter the Ancient Crypt.

#### **Tool Input:**
{
    "sourcenodename": "Old Graveyard", 
    "targetnodename": "Ancient Crypt", 
    "conditions": [ "Has the player obtained the Rusted Key?" ] 
}

#### **Outcome:**
- The **Old Graveyard** is now connected to the **Ancient Crypt**.
- The player cannot enter the crypt until they have obtained the Rusted Key.

---

#### **Example 2: Gaining Access to the Royal Chamber**
**Scenario:**  
To enter the Royal Chamber, the player must have:
1. Met Sir Ivan, the Wizard, who provides the key to the chamber.
2. Defeated the Elite Guards stationed outside.
3. Dispelled the magical barrier on the Royal Chamber doors.

#### **Tool Input:**
{
    "sourcenodename": "Castle Courtyard", 
    "targetnodename": "Royal Chamber", 
    "conditions": [ 
        "Has the player been granted the key by Sir Ivan, the Wizard?", 
        "Has the player defeated the Elite Guards?", 
        "Has the player dispelled the magical barrier?" 
        ] 
}

#### **Outcome:**
- The **Castle Courtyard** is now connected to the **Royal Chamber**.
- The player cannot enter until all conditions are fulfilled.
