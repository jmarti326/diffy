# Changelog

All notable changes to Diffy will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-02-10

### Added
- Initial release of Diffy
- Side-by-side text comparison with character-level diff highlighting
- Load files from disk for comparison
- Swap left/right content
- Clear all content
- Light/Dark theme toggle
- Copy functionality:
  - Copy left text
  - Copy right text
  - Copy unified diff format
  - Right-click context menu on diff lines
  - Multi-select with click and drag
  - Copy selected lines
- Statistics display (inserted, deleted, modified counts)
- Status bar with operation feedback

---

## Versioning Guide

### Version Format: MAJOR.MINOR.PATCH

- **MAJOR**: Breaking changes or significant redesigns
- **MINOR**: New features, backwards compatible
- **PATCH**: Bug fixes, backwards compatible

### Release Process

1. Update version in `src/Diffy/Diffy.csproj`
2. Update this CHANGELOG.md (move Unreleased items to new version)
3. Commit changes: `git commit -m "Release vX.Y.Z"`
4. Tag the release: `git tag -a vX.Y.Z -m "Version X.Y.Z"`
5. Push with tags: `git push origin main --tags`

[Unreleased]: https://github.com/jmarti326/diffy/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/jmarti326/diffy/releases/tag/v1.0.0
