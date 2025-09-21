# Unity Code Structure Analyzer

A lightweight .NET tool to extract and analyze C# code structure from Unity projects. Ideal for modders, developers, and teams building automated workflows.

## Overview

This utility uses Roslyn to parse C# source files in a Unity project folder (e.g., Assets/Scripts), listing classes, structs, methods, properties, fields, and constructors. It skips build artifacts and outputs a structured CSV for easy querying and analysis.

Useful for **Automated Pipelines**: Process extracted members for code generation, documentation, refactoring, or integration with CI/CD tools.

## Quick Start

1. Clone the repo:
   ```
   git clone https://github.com/yourusername/UnityMemberList.git
   cd UnityMemberList
   ```

2. Build:
   ```
   dotnet build
   ```

3. Run (provide path to scripts folder):
   ```
   dotnet run -- path/to/Assets/Scripts [output.csv]
   ```

Results in `output.csv` with columns: FilePath, ClassName, MemberName, MemberType.

## Usage Example

```
dotnet run -- /path/to/unity/project/Assets output_structure.csv
```

Outputs:
```
FilePath,ClassName,MemberName,MemberType
"SpawnableThing.cs","SpawnableThing","SpawnableThing","Class"
"SpawnableThing.cs","SpawnableThing","typeID","Field"
"SpawnableThing.cs","SpawnableThing","onCollect","Field"
"SpawnableThing.cs","SpawnableThing","type","Field"
"SpawnableThing.cs","SpawnableThing","OnSpawn","Method"
"SpawnableThing.cs","SpawnableThing","OnDespawn","Method"
```

## License

MIT - see [LICENSE.md](LICENSE.md).