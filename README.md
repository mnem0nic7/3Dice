# 3Dice - Multi-Platform Dice Roller

A cross-platform dice rolling application built with .NET MAUI, supporting multiple dice types for tabletop gaming and probability calculations.

## ?? Features

- **Multiple Dice Types**: Support for D4, D6, D8, D10, D12, D20, and D100 dice
- **Flexible Quantities**: Select different quantities for each dice type (up to 20 per type)
- **Visual Interface**: Clean, text-based interface optimized for mobile devices
- **Advanced Results**: Shows individual rolls and grand totals
- **Cross-Platform**: Runs on Android, iOS, and macOS

## ?? Platforms Supported

- ? **Android** (API 21+)
- ? **iOS** (11.0+)
- ? **macOS** (via Mac Catalyst 13.1+)

## ??? Built With

- **.NET 8**
- **.NET MAUI** (Multi-platform App UI)
- **C#** with XAML

## ?? How to Use

1. **Select Dice Types**: Choose from D4, D6, D8, D10, D12, D20, or D100
2. **Set Quantities**: Use +/- buttons to select how many of each dice type
3. **Roll**: Tap "?? Roll All Dice ??" to see results
4. **View Results**: See individual rolls and grand total
5. **Clear**: Use "Clear All" to reset selections

## ??? Building the Project

### Prerequisites
- Visual Studio 2022 with .NET MAUI workload
- .NET 8 SDK
- Android SDK (for Android builds)
- Xcode (for iOS/macOS builds, Mac only)

### Build Commands
```bash
# Restore packages
dotnet restore

# Build for Android
dotnet build -f net8.0-android

# Build for iOS
dotnet build -f net8.0-ios

# Build for macOS
dotnet build -f net8.0-maccatalyst
```

## ?? Example Usage

**Rolling 2D6 + 1D20:**
```
Results:
D6 (2x): [4, 6] = 10
D20: 15

?? Grand Total: 25
```

## ?? Development

The app uses a clean MVVM-like architecture:
- `MainPage.xaml` - UI layout
- `MainPage.xaml.cs` - Logic and event handling
- `Models/DiceType.cs` - Data model with INotifyPropertyChanged
- `Models/DiceGroup.cs` - Result grouping model

## ?? License

This project is open source. Feel free to use, modify, and distribute.

## ?? Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.