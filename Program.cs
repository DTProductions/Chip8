using SDL2;
using System;


namespace Chip8Interpreter {
	internal class Program {
		static void Main(string[] args) {
			if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_AUDIO) < 0) {
				throw new Exception(SDL.SDL_GetError());
			}

			SdlDisplay sdlDisplay = new SdlDisplay(64, 32, "Interpreter", 10);
			SdlKeypad keypad = new SdlKeypad();
			SdlAudioManager audioManager = new SdlAudioManager(220, 64);

			Interpreter interpreter = new Interpreter(sdlDisplay, keypad, "slipperyslope.ch8", audioManager, true, true, true);

			Decoder decoder = new Decoder(interpreter);

			while (!keypad.ClosedButtonPressed()) {
				keypad.HandleInput();
				decoder.ExecuteNextInstruction();
			}

			sdlDisplay.Dispose();
			SDL.SDL_Quit();
		}
	}
}

