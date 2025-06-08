using SDL2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Interpreter {
	internal class SdlDisplay : IDisposable {
		private nint window;
		private nint renderer;
		private bool[,] display;
		private int displayResizeFactor;
		public int Width { get; private set; }
		public int Height { get; private set; }

		public SdlDisplay(int width, int height, string title, int displayResizeFactor) {
			Width = width;
			Height = height;
			display = new bool[width, height];

			int resizedWidth = display.GetLength(0) * displayResizeFactor;
			int resizedHeight = display.GetLength(1) * displayResizeFactor;

			this.displayResizeFactor = displayResizeFactor;

			window = SDL.SDL_CreateWindow(title, SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, resizedWidth, resizedHeight, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

			if (window == IntPtr.Zero) {
				throw new InvalidOperationException(SDL.SDL_GetError());
			}

			renderer = SDL.SDL_CreateRenderer(window, -1,
													SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
													SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

			if (renderer == IntPtr.Zero) {
				throw new InvalidOperationException(SDL.SDL_GetError());
			}
		}

		public void Clear() {
			for (int i = 0; i < Width; i++) {
				for (int j = 0; j < Height; j++) {
					display[i, j] = false;
				}
			}
		}

		private void ClearRendererBackbuffer() {
			if (SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255) < 0) {
				throw new InvalidOperationException(SDL.SDL_GetError());
			}

			if (SDL.SDL_RenderClear(renderer) < 0) {
				throw new InvalidOperationException(SDL.SDL_GetError());
			}
		}

		public void Dispose() {
			SDL.SDL_DestroyRenderer(renderer);
			SDL.SDL_DestroyWindow(window);
		}

		public void Render() {
			ClearRendererBackbuffer();
			SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);

			for (int i = 0; i < display.GetLength(0); i++) {
				for (int j = 0; j < display.GetLength(1); j++) {
					if (display[i, j]) {

						var rect = new SDL.SDL_Rect {
							x = i * displayResizeFactor,
							y = j * displayResizeFactor,
							w = 10,
							h = 10
						};


						SDL.SDL_RenderFillRect(renderer, ref rect);
					}
				}
			}

			SDL.SDL_RenderPresent(renderer);
		}

		public bool GetPixel(int x, int y) {
			return display[x, y];
		}

		public void SetPixel(int x, int y, bool value) {
			display[x, y] = value;
		}
	}
}
