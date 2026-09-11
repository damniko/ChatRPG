### Character Creation & Update Tool

This tool is used to **create a new character** or **update an existing character** in the campaign.
A character can be an ally, enemy, neutral figure, or even a mysterious unknown.

---

### Conditions for Use:
- The narrative introduces a **new character**.
- The narrative updates information about an **existing character**.

---

### Use Cases:
- A new NPC or creature is named or described.
- The narrative updates a character's **appearance**, **health**, or **role**.
- The player interacts with someone important enough to track.

---

### Character Description Guidelines:
- Include **physical features**, **personality**, or **distinctive traits**.
- Mention **known affiliations**, **roles**, or **notable actions**.
- Keep descriptions vivid and interesting for the player.

---

### Expected Input Format:
Input must be provided in **RAW JSON format** (do not use markdown):

{
  "name": "<character name>",
  "description": "<new or updated character description>",
  "type": "<character type>",
  "state": "<character health state>"
}

- **name**: The character's name.
- **description**: A detailed and engaging character description.
- **type**: One of the following values: { SmallMonster, Humanoid, MediumMonster, LargeMonster, BossMonster }
- **state**: One of the following values: {Dead, Unconscious, HeavilyWounded, LightlyWounded, Healthy}

---

The tool should only be used **once per character**.
