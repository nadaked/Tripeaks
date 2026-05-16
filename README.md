# Tripeaks

Modern mobile-oriented Tripeaks foundation built with a clean gameplay core,
runtime board generation, editor tooling, and animation-first presentation.

## Current Foundation

- Clean gameplay core separated from Unity presentation code
- Undoable move pipeline with board, deck, waste, wild, and reward card actions
- Dynamic board creation from `BoardDefinition` assets
- Seeded card/deck generation with fixed tutorial openings when needed
- DOTween-based `GameAnimationDirector` for board, deck, waste, reveal, undo, and hint feedback
- Board definition editor tool for designer-friendly layout authoring
- GameServices package integration via `com.nadaked.game-services`
- 2D scalable card rendering with named rank/suit textures

## Architecture

- `Core`: cards, rules, deck state, move validation, action resolving, undo records
- `Application`: game state building, level data, card/deck generation
- `Presentation`: board, card, deck, and waste views
- `Animations`: visual orchestration through `GameAnimationDirector`
- `Editor`: board definition authoring tools
- `Services`: GameServices config and service factory assets

## Gameplay Features

- Normal cards
- Dual-rank cards
- Wild cards
- Reward cards such as +3/+5 deck additions
- Undo system with reverse animation support
- Deck/Waste system with animated dealing and accordion deck layout
- Invalid move and missed-play hint shake feedback
- Combo-friendly generated decks for smoother early gameplay

## Editor Tooling

The board definition editor now supports designer-facing layout work:

- Add normal, wild, and reward board cards directly in the scene
- Assign blockers from selected cards
- Duplicate and mirror selected card groups
- Generate `BoardDefinition` assets from scene-authored layouts
- Create common patterns such as daisy, stack, fan, pyramid, and tutorial arc layouts
- Scrollable/foldout UI for a simpler level design workflow

## GameServices

The project includes `nadaked/GameServices` through the Unity package manifest:

```json
"com.nadaked.game-services": "https://github.com/nadaked/GameServices.git?path=Packages/com.nadaked.game-services"
```

Project service assets live under `Assets/_Project/Services`, including config,
provider, audio, scene loading, save, and mock factory assets.

## Completed Planning Items

- Level editor tooling
- Animation polish
- GameServices package setup

## Planned Features

- Audio orchestration
- Broader combo and difficulty progression systems
- Mobile optimization
- Production end-of-level and replay flow
- Further blocker workflow polish in the board editor
