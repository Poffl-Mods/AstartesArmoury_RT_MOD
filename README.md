# Astartes Armoury

Author: Poffl

Astartes Armoury is a small add-on for [Sternab's Deathwatch mod](https://github.com/Sternab/RogueTrader-Deathwatch) for *Warhammer 40,000: Rogue Trader*. It adds three deliberately overpowered named weapons for a Deathwatch player character:

- **Vigil's Oath** — an Astartes bolt rifle whose hits build stacking critical-damage momentum.
- **Final Judgement** — a two-handed eviscerator with doubled Strength scaling, improved parry, and a three-strike execution attack.
- **God-Emperor's Wrath** — a heavy bolter whose rate of fire rises with every kill for the rest of the combat.

This mod is intentionally a Space Marine power fantasy. It is not balanced around the vanilla campaign's equipment curve.

## Requirements

- *Warhammer 40,000: Rogue Trader*
- [Deathwatch by Sternab](https://github.com/Sternab/RogueTrader-Deathwatch) — required

Deathwatch is not included in this repository or redistributed by Astartes Armoury. Install and maintain it separately.

## Installation

1. Install the required Deathwatch mod.
2. Extract the Astartes Armoury release into the game's user `Modifications` directory.
3. Enable both `Deathwatch` and `AstartesArmoury_RT_MOD` in the Owlcat modification settings.

The three weapons are granted once, unequipped, after an area loads when the actual main player character has `DW_AstartesRace`. A save-persistent marker makes the runtime grant idempotent and also supports existing Deathwatch saves.

## Building from source

Use Owlcat's official Rogue Trader modification template with Unity `6000.0.64f1`. Place or link `Assets/Modifications/AstartesArmoury_RT_MOD` into the template's matching `Assets/Modifications` directory, then build `AstartesArmoury_RT_MOD` with Owlcat Tools. Deathwatch remains a separate manifest dependency and must not be copied into this project.

## Credits

- Sternab for the open-source Deathwatch mod and its playable Astartes foundation.
- Owlcat Games for *Warhammer 40,000: Rogue Trader* and the official modification template.
