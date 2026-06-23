# ZnaykoAI

A C# application powered by AI, utilizing the OpenRouter API for intelligent agent capabilities.

## Overview

ZnaykoAI is a project built with modern web technologies combining C#, HTML, and CSS to create an AI-driven agent system. The application leverages OpenRouter's API to provide seamless AI integration and intelligent agent functionality.

## Tech Stack

- **Backend**: C# (75.6%)
- **Frontend**: HTML (22.2%), CSS (1.9%), JavaScript (0.3%)

## Prerequisites

### Required: OpenRouter API Key

**This application requires an OpenRouter API key to work and debug properly.** 

Before running or debugging the application, you must:

1. **Sign up at [OpenRouter](https://openrouter.ai/)**
2. **Generate an API key** from your account settings
3. **Configure the API key** in your application settings (see [Configuration](#configuration) section below)

Without a valid OpenRouter API key, the application will not be able to function.

## Installation

### Clone the Repository

```bash
git clone https://github.com/manol-aleksandrov/ZnaykoAI.git
cd ZnaykoAI
```

### Build the Project

```bash
dotnet build
```

## Configuration

### Setting Up OpenRouter API Key

1. Obtain your OpenRouter API key from [openrouter.ai](https://openrouter.ai/)
2. Configure the API key using one of the following methods:

   - **Environment Variable** (Recommended):
     ```bash
     export OPENROUTER_API_KEY=your_api_key_here
     ```
   
   - **Application Settings**:
     Update the configuration file with your API key
   
   - **Development/Debug Mode**:
     Add the key to your local development environment

## Running the Application

### Development

```bash
dotnet run
```

### Debug Mode

To debug the application, ensure your OpenRouter API key is properly configured as described in the [Configuration](#configuration) section.

```bash
dotnet run --configuration Debug
```

## Project Structure

The project is organized as follows:

- **C# Backend**: Core application logic and AI agent implementation
- **Web Interface**: HTML and CSS for the user interface
- **Scripts**: JavaScript for client-side functionality

## Usage

[Add usage instructions here as your project develops]

## API Integration

This application uses the OpenRouter API to access various AI models and agent capabilities. For more information about OpenRouter and available models, visit [OpenRouter Documentation](https://openrouter.ai/docs).

## Troubleshooting

### API Key Issues
- Ensure your OpenRouter API key is valid and not expired
- Verify the API key is properly configured in your environment
- Check that your OpenRouter account has sufficient credits

### Build Errors
- Ensure you have .NET installed: `dotnet --version`
- Clean and rebuild: `dotnet clean && dotnet build`

### Runtime Errors
- Verify all dependencies are installed: `dotnet restore`
- Check that the OpenRouter API is accessible from your network

## License

[Add your license information here]

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues, questions, or contributions, please open an issue on the [GitHub repository](https://github.com/manol-aleksandrov/ZnaykoAI).

---

**⚠️ Important: Remember to set your OpenRouter API key before running or debugging this application.**
