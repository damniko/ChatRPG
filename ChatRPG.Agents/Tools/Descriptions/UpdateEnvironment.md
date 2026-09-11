This tool must be used to **create a new environment** or **update an existing environment** in the campaign.  

**Example Usage:**  
The narrative text mentions a new environment or contains changes to an existing environment.

---

### **What is an Environment?**
An environment refers to a **place**, **location**, or **area** that is well enough defined to warrant its own description.  
Such places could include:
- A **landmark** with its own history.
- A **building** where story events take place.
- A larger place like a **magical forest**.

---

### **Tool Input Format**
Input to this tool must be in the following **RAW JSON format**:
{
    "name": "<environment name>",
    "description": "<new or updated environment description>",
    "isPlayerHere": <true if the Player character is currently at this environment, false otherwise>
}

### **Description of an Environment**
- The **description** could cover:
  - Its **physical characteristics**.
  - Its **significance** in the story.
  - The **creatures** that inhabit it.
  - The **weather** or other descriptive features.

The goal is to provide the Player with **useful information** about the places they travel to, 
while keeping the locations' descriptions **interesting**, **mysterious**, and **engaging**.

---

### **Important Notes**
- The tool should **only be used once** per environment to avoid redundancy and maintain clarity in the narrative.
