Use the battle tool to resolve battle or combat between two participants. A participant is 
a single character and cannot be a combination of characters. If there are more 
than two participants, the tool must be used once per attacker to give everyone a chance at fighting. 

The battle tool will give each participant a chance to fight the other participant. The tool should 
also be used when an attack can be mitigated or dodged by the involved participants. It is also 
possible for either or both participants to miss. A hit chance specifier will help adjust the chance 
that a participant gets to retaliate.

### Example Usage:
- **Scenario 1: Two Combatants**  
  - There are only two combatants.  
  - Call the tool **only ONCE**, since both characters get an attack.  

- **Scenario 2: Three Combatants (Player vs. Two Assassins)**  
  - The battle tool is called first with the Player's character as **participant one**  
    and one of the assassins as **participant two**.  
  - The Player has a high chance of hitting the assassin.  
  - The assassins must be precise, making their hits harder to land, but they deal high damage when successful.  
  - If **participant one hits participant two** and **participant two misses participant one**,  
    this round is resolved.  
  - The tool is then called **again** with the Player’s character as participant one and the other assassin as participant two.  
  - Since participant one has already hit once in this battle, a **penalty is imposed** on their hit chance,  
    which accumulates for each successful attack in the battle.  

### Damage Severity:
- The **damage severity** describes how powerful an attack is, derived from the narrative description.
- If participants engage in a friendly sparring fight, do not intend to hurt, or are in a mock battle,  
  the **damage severity is `<harmless>`**.
- If no direct description is available, estimate the impact of an attack based on the **character type**  
  and their **description**.

### Input Format:
Input to this tool must be in the following **RAW JSON format** (do not use markdown):
{
    "participant1": {
        "name": "<name of participant one>",
        "description": "<description of participant one>"
    },
    "participant2": {
        "name": "<name of participant two>",
        "description": "<description of participant two>"
    },
    "participant1HitChance": "<hit chance specifier for participant one>",
    "participant2HitChance": "<hit chance specifier for participant two>",
    "participant1DamageSeverity": "<damage severity for participant one>",
    "participant2DamageSeverity": "<damage severity for participant two>"
}

### Accepted Values:
- **`participant#HitChance` specifiers:** `{high, medium, low, impossible}`
- **`participant#DamageSeverity` values:** `{harmless, low, medium, high, extraordinary}`

The narrative battle **ends** when each character has had the chance to attack another 
character **at most once**.
