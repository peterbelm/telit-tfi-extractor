# telit-tfi-extractor
C# utility for extracting Telit firmware from 'TFI' executables.

## Telit Firmware Image (TFI)
New Telit module firmware images are distributed as Telit Firmware Images (TFI - assumed name) which are executables with the firmware image files and a table of contents (TOC) appended to the executable binary.

## Table of Contents (TOC)
Appears at the very end of the file. Contains a Header, a number of Elements, and a Footer.

The first element is always the executable itself, the second is a 16-byte marker 'TELITTFITELITTFI', and the final element is for the TOC itself. The final element is always **version 1**.

### Header
| Offset | Length | Description |
| - | - | - |
| 0x0000 | 0x0004 | Magic (0x12345678) |
| 0x0004 | 0x0004 | Version (either 1 or 2) |
| 0x0008 | 0x0004 | Number of elements |

### Element V1
| Offset | Length | Description |
| - | - | - |
| 0x0000 | 0x0004 | Tag |
| 0x0004 | 0x0004 | Offset |
| 0x0008 | 0x0004 | Size |
| 0x000C | 0x0010 | *Group or destination?* |
| 0x001C | 0x0080 | Filename |

### Element V2
| Offset | Length | Description |
| - | - | - |
| 0x0000 | 0x0004 | Tag |
| 0x0004 | 0x0004 | Offset |
| 0x0008 | 0x0004 | Size |
| 0x000C | 0x0010 | *Group or destination?* |
| 0x001C | 0x04A0 | Filename |

### Footer
| Offset | Length | Description |
| - | - | - |
| 0x0000 | 0x0004 | Checksum over TOC elements |
| 0x0004 | 0x0004 | Magic (-0x12345678) |