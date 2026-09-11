This tool must be used to **add a new plot point** (node) to the narrative graph while 
structuring an adventure scenario.  
Each node represents a **key location**, **event**, or **point of interest** in the story. 
The tool should be used whenever you determine that a new plot point needs to be introduced 
in the graph based on the provided scenario document.

### **Node Contents**
Each node should contain:
- A **unique name**, which describes the location or plot point.
- A **story content description**, detailing the narrative aspects, such as key NPCs, obstacles, or important discoveries.
- **Edges**, which represent the paths leading to or from other existing nodes.
    - Each edge includes **conditions** that must be fulfilled before traversal is allowed.
    - Edges connect an existing source node to the new target node or vice versa to ensure logical progression.

---

### **After Using the Tool**
After calling this tool, you will receive an updated string representation of the narrative graph, 
showing the newly added node and its connections.  
This allows you to **verify relationships** between story points and ensure **correct structuring**.

---

### **Usage Format**
- **Do not use markdown!**  
- The tool requires **valid JSON input**, structured as follows:
{
    "name": "a unique name of the node based on the location or plot point within the scenario document",
    "storycontent": "the story content of the relevant details such as a description of the plot point/location, key NPCs, obstacles, or possible discoveries, etc.",
    "edges": [
        {
            "conditions": [
                "condition 1 for traversing the edge",
                "condition 2 for traversing the edge"
            ],
            "sourcenodename": "the name of the source node that should be connected using this edge. This node can already exist in the graph or it can be this node, if this node is the source",
            "targetnodename": "the name of the target node that should be connected using this edge. This node can already exist in the graph or it can be this node, if this node is the target"
        }
    ]
}

Each edge must include a **list of conditions** and connect either from or to an existing node to maintain coherence in the narrative structure.  
These conditions must be formulated as **short easy-to-answer questions**.

---

### **Example Usage**
#### **Example 1: Gaining Information on The Abandoned Ruins**
**Scenario Context:**  
The player is currently at "**The Village of Eldermere**". A new story point is being introduced: "**The Abandoned Ruins**", which contains an ancient shrine with hidden inscriptions. The player can only proceed if they have:
- Spoken to the village elder.
- Removed a large boulder blocking the path.

#### **Tool Call Example:**
{
    "name": "The Abandoned Ruins",
    "storycontent": "A crumbling stone structure overgrown with vines, hiding an ancient shrine with faded inscriptions. The air is thick with mystery, and a sense of forgotten history lingers. Possible discoveries include ancient artifacts and hidden passages.",
    "edges": [
        {
            "conditions": [
                "Has the player spoken to the Village Elder?",
                "Has the player removed the large boulder?"
            ],
            "sourcenodename": "The Village of Eldermere",
            "targetnodename": "The Abandoned Ruins"
        }
    ]
}

#### **Expected Outcome:**
- The tool returns an updated string representation of the graph, now including "**The Abandoned Ruins**" as a new node, connected to "**The Village of Eldermere**" via an edge with the conditions:
  - "Has the player spoken to the Village Elder?"
  - "Has the player removed the large boulder?"
- You can now verify the structure and ensure that traversal logic remains consistent with the scenario documents.

---

#### **Example 2: Entering the Forbidden Archives (No Conditions Required)**
**Scenario Context:**  
A new node is added when the player discovers the **Forbidden Archives**, an ancient library containing lost knowledge.

#### **Tool Call Example:**
{
    "name": "Forbidden Archives",
    "storycontent": "A vast underground library filled with crumbling tomes, forbidden knowledge, and the echoes of long-forgotten scholars. Strange symbols glow faintly on the walls, hinting at secrets waiting to be uncovered.",
    "edges": [
        {
            "conditions": [],
            "sourcenodename": "Grand Library",
            "targetnodename": "Forbidden Archives"
        }
    ]
}

#### **Outcome:**
- The **Forbidden Archives** is introduced as a new story node.
- The **Grand Library** is directly connected to it without conditions, meaning the player can freely enter the archives.
- The archives can now serve as a new exploration point with potential clues, puzzles, or hidden dangers.
