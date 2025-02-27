// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Imcodec.Math;

/// <summary>
/// Represents a 32-bit color (4 bytes) in the form of RGBA (in byte order: R, G, B, A).
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 4)]
public partial struct Color : IEquatable<Color>, IFormattable {

    private const string ToStringFormat = "A:{0} R:{1} G:{2} B:{3}";

    /// <summary>
    /// The red component of the color.
    /// </summary>
    public byte R;

    /// <summary>
    /// The green component of the color.
    /// </summary>
    public byte G;

    /// <summary>
    /// The blue component of the color.
    /// </summary>
    public byte B;

    /// <summary>
    /// The alpha component of the color.
    /// </summary>
    public byte A;

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="value">The value that will be assigned to all components.</param>
    public Color(byte value) => A = R = G = B = value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="value">The value that will be assigned to all components.</param>
    public Color(float value) => A = R = G = B = ToByte(value);

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="red">The red component of the color.</param>
    /// <param name="green">The green component of the color.</param>
    /// <param name="blue">The blue component of the color.</param>
    /// <param name="alpha">The alpha component of the color.</param>
    public Color(byte red, byte green, byte blue, byte alpha) {
        R = red;
        G = green;
        B = blue;
        A = alpha;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.  Alpha is set to 255.
    /// </summary>
    /// <param name="red">The red component of the color.</param>
    /// <param name="green">The green component of the color.</param>
    /// <param name="blue">The blue component of the color.</param>
    public Color(byte red, byte green, byte blue) {
        R = red;
        G = green;
        B = blue;
        A = 255;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.  Passed values are clamped within byte range.
    /// </summary>
    /// <param name="red">The red component of the color.</param>
    /// <param name="green">The green component of the color.</param>
    /// <param name="blue">The blue component of the color.</param>
    /// <param name="alpha">The alpha component of the color</param>
    public Color(int red, int green, int blue, int alpha) {
        R = ToByte(red);
        G = ToByte(green);
        B = ToByte(blue);
        A = ToByte(alpha);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.  Alpha is set to 255.  Passed values are clamped within byte range.
    /// </summary>
    /// <param name="red">The red component of the color.</param>
    /// <param name="green">The green component of the color.</param>
    /// <param name="blue">The blue component of the color.</param>
    public Color(int red, int green, int blue)
        : this(red, green, blue, 255) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="red">The red component of the color.</param>
    /// <param name="green">The green component of the color.</param>
    /// <param name="blue">The blue component of the color.</param>
    /// <param name="alpha">The alpha component of the color.</param>
    public Color(float red, float green, float blue, float alpha) {
        R = ToByte(red);
        G = ToByte(green);
        B = ToByte(blue);
        A = ToByte(alpha);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.  Alpha is set to 255.
    /// </summary>
    /// <param name="red">The red component of the color.</param>
    /// <param name="green">The green component of the color.</param>
    /// <param name="blue">The blue component of the color.</param>
    public Color(float red, float green, float blue) {
        R = ToByte(red);
        G = ToByte(green);
        B = ToByte(blue);
        A = 255;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="value">The red, green, blue, and alpha components of the color.</param>
    public Color(Vector4 value) {
        R = ToByte(value.X);
        G = ToByte(value.Y);
        B = ToByte(value.Z);
        A = ToByte(value.W);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="value">The red, green, and blue components of the color.</param>
    /// <param name="alpha">The alpha component of the color.</param>
    public Color(Vector3 value, float alpha) {
        R = ToByte(value.X);
        G = ToByte(value.Y);
        B = ToByte(value.Z);
        A = ToByte(alpha);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct. Alpha is set to 255.
    /// </summary>
    /// <param name="value">The red, green, and blue components of the color.</param>
    public Color(Vector3 value) {
        R = ToByte(value.X);
        G = ToByte(value.Y);
        B = ToByte(value.Z);
        A = 255;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="rgba">A packed integer containing all four color components in RGBA order.</param>
    public Color(uint rgba) {
        A = (byte) ((rgba >> 24) & 255);
        B = (byte) ((rgba >> 16) & 255);
        G = (byte) ((rgba >> 8) & 255);
        R = (byte) (rgba & 255);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="rgba">A packed integer containing all four color components in RGBA order.</param>
    public Color(int rgba) {
        A = (byte) ((rgba >> 24) & 255);
        B = (byte) ((rgba >> 16) & 255);
        G = (byte) ((rgba >> 8) & 255);
        R = (byte) (rgba & 255);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="values">The values to assign to the red, green, and blue, alpha components of the color. This must be an array with four elements.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
    public Color(float[] values) {
        if (values != null) {
            if (values.Length != 4) {
                throw new ArgumentOutOfRangeException(nameof(values), "There must be four and only four input values for Color.");
            }

            R = ToByte(values[0]);
            G = ToByte(values[1]);
            B = ToByte(values[2]);
            A = ToByte(values[3]);
        }
        else {
            throw new ArgumentNullException(nameof(values));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="values">The values to assign to the alpha, red, green, and blue components of the color. This must be an array with four elements.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
    public Color(byte[] values) {
        if (values != null) {
            if (values.Length != 4) {
                throw new ArgumentOutOfRangeException(nameof(values), "There must be four and only four input values for Color.");
            }

            R = values[0];
            G = values[1];
            B = values[2];
            A = values[3];
        }
        else {
            throw new ArgumentNullException(nameof(values));
        }
    }

    /// <summary>
    /// Gets or sets the component at the specified index.
    /// </summary>
    /// <value>The value of the alpha, red, green, or blue component, depending on the index.</value>
    /// <param name="index">The index of the component to access. Use 0 for the alpha component, 1 for the red component, 2 for the green component, and 3 for the blue component.</param>
    /// <returns>The value of the component at the specified index.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>
    public byte this[int index] {
        readonly get => index switch {
            0 => R,
            1 => G,
            2 => B,
            3 => A,
            _ => throw new ArgumentOutOfRangeException(nameof(index), "Indices for Color run from 0 to 3, inclusive."),
        };

        set {
            switch (index) {
                case 0: R = value; break;
                case 1: G = value; break;
                case 2: B = value; break;
                case 3: A = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(index), "Indices for Color run from 0 to 3, inclusive.");
            }
        }
    }

    /// <summary>
    /// Converts the color into a packed integer.
    /// </summary>
    /// <returns>A packed integer containing all four color components.</returns>
    public readonly int ToBgra() {
        int value = B;
        value |= G << 8;
        value |= R << 16;
        value |= A << 24;

        return (int) value;
    }

    /// <summary>
    /// Converts the color into a packed integer.
    /// </summary>
    /// <returns>A packed integer containing all four color components.</returns>
    public readonly int ToRgba() {
        int value = R;
        value |= G << 8;
        value |= B << 16;
        value |= A << 24;

        return (int) value;
    }

    /// <summary>
    /// Converts the color into a packed integer.
    /// </summary>
    /// <returns>A packed integer containing all four color components.</returns>
    public readonly int ToAbgr() {
        int value = A;
        value |= B << 8;
        value |= G << 16;
        value |= R << 24;

        return (int) value;
    }

    /// <summary>
    /// Creates an array containing the elements of the color.
    /// </summary>
    /// <returns>A four-element array containing the components of the color in RGBA order.</returns>
    public readonly byte[] ToArray() => [R, G, B, A];

    /// <summary>
    /// Gets the brightness.
    /// </summary>
    /// <returns>The Hue-Saturation-Brightness (HSB) brightness for this <see cref="Color"/></returns>
    public readonly float GetBrightness() {
        float r = (float) R / 255.0f;
        float g = (float) G / 255.0f;
        float b = (float) B / 255.0f;

        float max, min;

        max = r; min = r;

        if (g > max) {
            max = g;
        }

        if (b > max) {
            max = b;
        }

        if (g < min) {
            min = g;
        }

        if (b < min) {
            min = b;
        }

        return (max + min) / 2;
    }

    /// <summary>
    /// Gets the hue.
    /// </summary>
    /// <returns>The Hue-Saturation-Brightness (HSB) hue for this <see cref="Color"/></returns>
    public readonly float GetHue() {
        if (R == G && G == B) {
            return 0; // 0 makes as good an UNDEFINED value as any
        }

        float r = (float) R / 255.0f;
        float g = (float) G / 255.0f;
        float b = (float) B / 255.0f;

        float max, min;
        float delta;
        float hue = 0.0f;

        max = r; min = r;

        if (g > max) {
            max = g;
        }

        if (b > max) {
            max = b;
        }

        if (g < min) {
            min = g;
        }

        if (b < min) {
            min = b;
        }

        delta = max - min;

        if (r == max) {
            hue = (g - b) / delta;
        }
        else if (g == max) {
            hue = 2 + (b - r) / delta;
        }
        else if (b == max) {
            hue = 4 + (r - g) / delta;
        }
        hue *= 60;

        if (hue < 0.0f) {
            hue += 360.0f;
        }
        return hue;
    }

    /// <summary>
    /// Gets the saturation.
    /// </summary>
    /// <returns>The Hue-Saturation-Brightness (HSB) saturation for this <see cref="Color"/></returns>
    public readonly float GetSaturation() {
        float r = (float) R / 255.0f;
        float g = (float) G / 255.0f;
        float b = (float) B / 255.0f;

        float max, min;
        float l, s = 0;

        max = r; min = r;

        if (g > max) {
            max = g;
        }

        if (b > max) {
            max = b;
        }

        if (g < min) {
            min = g;
        }

        if (b < min) {
            min = b;
        }

        // if max == min, then there is no color and
        // the saturation is zero.
        //
        if (max != min) {
            l = (max + min) / 2;

            if (l <= .5) {
                s = (max - min) / (max + min);
            }
            else {
                s = (max - min) / (2 - max - min);
            }
        }
        return s;
    }

    /// <summary>
    /// Adds two colors.
    /// </summary>
    /// <param name="left">The first color to add.</param>
    /// <param name="right">The second color to add.</param>
    /// <param name="result">When the method completes, completes the sum of the two colors.</param>
    public static void Add(ref Color left, ref Color right, out Color result) {
        result.A = (byte) (left.A + right.A);
        result.R = (byte) (left.R + right.R);
        result.G = (byte) (left.G + right.G);
        result.B = (byte) (left.B + right.B);
    }

    /// <summary>
    /// Adds two colors.
    /// </summary>
    /// <param name="left">The first color to add.</param>
    /// <param name="right">The second color to add.</param>
    /// <returns>The sum of the two colors.</returns>
    public static Color Add(Color left, Color right) => new(left.R + right.R, left.G + right.G, left.B + right.B, left.A + right.A);

    /// <summary>
    /// Subtracts two colors.
    /// </summary>
    /// <param name="left">The first color to subtract.</param>
    /// <param name="right">The second color to subtract.</param>
    /// <param name="result">WHen the method completes, contains the difference of the two colors.</param>
    public static void Subtract(ref Color left, ref Color right, out Color result) {
        result.A = (byte) (left.A - right.A);
        result.R = (byte) (left.R - right.R);
        result.G = (byte) (left.G - right.G);
        result.B = (byte) (left.B - right.B);
    }

    /// <summary>
    /// Subtracts two colors.
    /// </summary>
    /// <param name="left">The first color to subtract.</param>
    /// <param name="right">The second color to subtract</param>
    /// <returns>The difference of the two colors.</returns>
    public static Color Subtract(Color left, Color right) => new(left.R - right.R, left.G - right.G, left.B - right.B, left.A - right.A);

    /// <summary>
    /// Modulates two colors.
    /// </summary>
    /// <param name="left">The first color to modulate.</param>
    /// <param name="right">The second color to modulate.</param>
    /// <param name="result">When the method completes, contains the modulated color.</param>
    public static void Modulate(ref Color left, ref Color right, out Color result) {
        result.A = (byte) (left.A * right.A / 255.0f);
        result.R = (byte) (left.R * right.R / 255.0f);
        result.G = (byte) (left.G * right.G / 255.0f);
        result.B = (byte) (left.B * right.B / 255.0f);
    }

    /// <summary>
    /// Modulates two colors.
    /// </summary>
    /// <param name="left">The first color to modulate.</param>
    /// <param name="right">The second color to modulate.</param>
    /// <returns>The modulated color.</returns>
    public static Color Modulate(Color left, Color right) => new(left.R * right.R, left.G * right.G, left.B * right.B, left.A * right.A);

    /// <summary>
    /// Scales a color.
    /// </summary>
    /// <param name="value">The color to scale.</param>
    /// <param name="scale">The amount by which to scale.</param>
    /// <param name="result">When the method completes, contains the scaled color.</param>
    public static void Scale(ref Color value, float scale, out Color result) {
        result.A = (byte) (value.A * scale);
        result.R = (byte) (value.R * scale);
        result.G = (byte) (value.G * scale);
        result.B = (byte) (value.B * scale);
    }

    /// <summary>
    /// Scales a color.
    /// </summary>
    /// <param name="value">The color to scale.</param>
    /// <param name="scale">The amount by which to scale.</param>
    /// <returns>The scaled color.</returns>
    public static Color Scale(Color value, float scale) => new((byte) (value.R * scale), (byte) (value.G * scale), (byte) (value.B * scale), (byte) (value.A * scale));

    /// <summary>
    /// Negates a color.
    /// </summary>
    /// <param name="value">The color to negate.</param>
    /// <param name="result">When the method completes, contains the negated color.</param>
    public static void Negate(ref Color value, out Color result) {
        result.A = (byte) (255 - value.A);
        result.R = (byte) (255 - value.R);
        result.G = (byte) (255 - value.G);
        result.B = (byte) (255 - value.B);
    }

    /// <summary>
    /// Negates a color.
    /// </summary>
    /// <param name="value">The color to negate.</param>
    /// <returns>The negated color.</returns>
    public static Color Negate(Color value) => new(255 - value.R, 255 - value.G, 255 - value.B, 255 - value.A);

    /// <summary>
    /// Restricts a value to be within a specified range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="result">When the method completes, contains the clamped value.</param>
    public static void Clamp(ref Color value, ref Color min, ref Color max, out Color result) {
        byte alpha = value.A;
        alpha = (alpha > max.A) ? max.A : alpha;
        alpha = (alpha < min.A) ? min.A : alpha;

        byte red = value.R;
        red = (red > max.R) ? max.R : red;
        red = (red < min.R) ? min.R : red;

        byte green = value.G;
        green = (green > max.G) ? max.G : green;
        green = (green < min.G) ? min.G : green;

        byte blue = value.B;
        blue = (blue > max.B) ? max.B : blue;
        blue = (blue < min.B) ? min.B : blue;

        result = new Color(red, green, blue, alpha);
    }

    /// <summary>
    /// Computes the premultiplied value of the provided color.
    /// </summary>
    /// <param name="value">The non-premultiplied value.</param>
    /// <param name="result">The premultiplied result.</param>
    public static void Premultiply(ref Color value, out Color result) {
        var a = value.A / (255f * 255f);
        result.A = value.A;
        result.R = ToByte(value.R * a);
        result.G = ToByte(value.G * a);
        result.B = ToByte(value.B * a);
    }

    /// <summary>
    /// Computes the premultiplied value of the provided color.
    /// </summary>
    /// <param name="value">The non-premultiplied value.</param>
    /// <returns>The premultiplied result.</returns>
    public static Color Premultiply(Color value) {
        Premultiply(ref value, out var result);
        return result;
    }

    /// <summary>
    /// Converts the color from a packed BGRA integer.
    /// </summary>
    /// <param name="color">A packed integer containing all four color components in BGRA order</param>
    /// <returns>A color.</returns>
    public static Color FromBgra(int color) => new((byte) ((color >> 16) & 255), (byte) ((color >> 8) & 255), (byte) (color & 255), (byte) ((color >> 24) & 255));

    /// <summary>
    /// Converts the color from a packed BGRA integer.
    /// </summary>
    /// <param name="color">A packed integer containing all four color components in BGRA order</param>
    /// <returns>A color.</returns>
    public static Color FromBgra(uint color) => FromBgra(unchecked((int) color));

    /// <summary>
    /// Converts the color from a packed ABGR integer.
    /// </summary>
    /// <param name="color">A packed integer containing all four color components in ABGR order</param>
    /// <returns>A color.</returns>
    public static Color FromAbgr(int color) => new((byte) (color >> 24), (byte) (color >> 16), (byte) (color >> 8), (byte) color);

    /// <summary>
    /// Converts the color from a packed ABGR integer.
    /// </summary>
    /// <param name="color">A packed integer containing all four color components in ABGR order</param>
    /// <returns>A color.</returns>
    public static Color FromAbgr(uint color) => FromAbgr(unchecked((int) color));

    /// <summary>
    /// Converts the color from a packed BGRA integer.
    /// </summary>
    /// <param name="color">A packed integer containing all four color components in RGBA order</param>
    /// <returns>A color.</returns>
    public static Color FromRgba(int color) => new(color);

    /// <summary>
    /// Converts the color from a packed BGRA integer.
    /// </summary>
    /// <param name="color">A packed integer containing all four color components in RGBA order</param>
    /// <returns>A color.</returns>
    public static Color FromRgba(uint color) => new(color);

    /// <summary>
    /// Restricts a value to be within a specified range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>The clamped value.</returns>
    public static Color Clamp(Color value, Color min, Color max) {
        Clamp(ref value, ref min, ref max, out var result);
        return result;
    }

    /// <summary>
    /// Returns a color containing the smallest components of the specified colors.
    /// </summary>
    /// <param name="left">The first source color.</param>
    /// <param name="right">The second source color.</param>
    /// <param name="result">When the method completes, contains an new color composed of the largest components of the source colors.</param>
    public static void Max(ref Color left, ref Color right, out Color result) {
        result.A = (left.A > right.A) ? left.A : right.A;
        result.R = (left.R > right.R) ? left.R : right.R;
        result.G = (left.G > right.G) ? left.G : right.G;
        result.B = (left.B > right.B) ? left.B : right.B;
    }

    /// <summary>
    /// Returns a color containing the largest components of the specified colorss.
    /// </summary>
    /// <param name="left">The first source color.</param>
    /// <param name="right">The second source color.</param>
    /// <returns>A color containing the largest components of the source colors.</returns>
    public static Color Max(Color left, Color right) {
        Max(ref left, ref right, out var result);
        return result;
    }

    /// <summary>
    /// Returns a color containing the smallest components of the specified colors.
    /// </summary>
    /// <param name="left">The first source color.</param>
    /// <param name="right">The second source color.</param>
    /// <param name="result">When the method completes, contains an new color composed of the smallest components of the source colors.</param>
    public static void Min(ref Color left, ref Color right, out Color result) {
        result.A = (left.A < right.A) ? left.A : right.A;
        result.R = (left.R < right.R) ? left.R : right.R;
        result.G = (left.G < right.G) ? left.G : right.G;
        result.B = (left.B < right.B) ? left.B : right.B;
    }

    /// <summary>
    /// Returns a color containing the smallest components of the specified colors.
    /// </summary>
    /// <param name="left">The first source color.</param>
    /// <param name="right">The second source color.</param>
    /// <returns>A color containing the smallest components of the source colors.</returns>
    public static Color Min(Color left, Color right) {
        Min(ref left, ref right, out var result);
        return result;
    }

    /// <summary>
    /// Adjusts the contrast of a color.
    /// </summary>
    /// <param name="value">The color whose contrast is to be adjusted.</param>
    /// <param name="contrast">The amount by which to adjust the contrast.</param>
    /// <param name="result">When the method completes, contains the adjusted color.</param>
    public static void AdjustContrast(ref Color value, float contrast, out Color result) {
        result.A = value.A;
        result.R = ToByte(0.5f + contrast * (value.R / 255.0f - 0.5f));
        result.G = ToByte(0.5f + contrast * (value.G / 255.0f - 0.5f));
        result.B = ToByte(0.5f + contrast * (value.B / 255.0f - 0.5f));
    }

    /// <summary>
    /// Adjusts the contrast of a color.
    /// </summary>
    /// <param name="value">The color whose contrast is to be adjusted.</param>
    /// <param name="contrast">The amount by which to adjust the contrast.</param>
    /// <returns>The adjusted color.</returns>
    public static Color AdjustContrast(Color value, float contrast) => new(
            ToByte(0.5f + contrast * (value.R / 255.0f - 0.5f)),
            ToByte(0.5f + contrast * (value.G / 255.0f - 0.5f)),
            ToByte(0.5f + contrast * (value.B / 255.0f - 0.5f)),
            value.A);

    /// <summary>
    /// Adjusts the saturation of a color.
    /// </summary>
    /// <param name="value">The color whose saturation is to be adjusted.</param>
    /// <param name="saturation">The amount by which to adjust the saturation.</param>
    /// <param name="result">When the method completes, contains the adjusted color.</param>
    public static void AdjustSaturation(ref Color value, float saturation, out Color result) {
        float grey = value.R / 255.0f * 0.2125f + value.G / 255.0f * 0.7154f + value.B / 255.0f * 0.0721f;

        result.A = value.A;
        result.R = ToByte(grey + saturation * (value.R / 255.0f - grey));
        result.G = ToByte(grey + saturation * (value.G / 255.0f - grey));
        result.B = ToByte(grey + saturation * (value.B / 255.0f - grey));
    }

    /// <summary>
    /// Adjusts the saturation of a color.
    /// </summary>
    /// <param name="value">The color whose saturation is to be adjusted.</param>
    /// <param name="saturation">The amount by which to adjust the saturation.</param>
    /// <returns>The adjusted color.</returns>
    public static Color AdjustSaturation(Color value, float saturation) {
        float grey = value.R / 255.0f * 0.2125f + value.G / 255.0f * 0.7154f + value.B / 255.0f * 0.0721f;

        return new Color(
            ToByte(grey + saturation * (value.R / 255.0f - grey)),
            ToByte(grey + saturation * (value.G / 255.0f - grey)),
            ToByte(grey + saturation * (value.B / 255.0f - grey)),
            value.A);
    }

    /// <summary>
    /// Adds two colors.
    /// </summary>
    /// <param name="left">The first color to add.</param>
    /// <param name="right">The second color to add.</param>
    /// <returns>The sum of the two colors.</returns>
    public static Color operator +(Color left, Color right) => new(left.R + right.R, left.G + right.G, left.B + right.B, left.A + right.A);

    /// <summary>
    /// Assert a color (return it unchanged).
    /// </summary>
    /// <param name="value">The color to assert (unchanged).</param>
    /// <returns>The asserted (unchanged) color.</returns>
    public static Color operator +(Color value) => value;

    /// <summary>
    /// Subtracts two colors.
    /// </summary>
    /// <param name="left">The first color to subtract.</param>
    /// <param name="right">The second color to subtract.</param>
    /// <returns>The difference of the two colors.</returns>
    public static Color operator -(Color left, Color right) => new(left.R - right.R, left.G - right.G, left.B - right.B, left.A - right.A);

    /// <summary>
    /// Negates a color.
    /// </summary>
    /// <param name="value">The color to negate.</param>
    /// <returns>A negated color.</returns>
    public static Color operator -(Color value) => new(-value.R, -value.G, -value.B, -value.A);

    /// <summary>
    /// Scales a color.
    /// </summary>
    /// <param name="scale">The factor by which to scale the color.</param>
    /// <param name="value">The color to scale.</param>
    /// <returns>The scaled color.</returns>
    public static Color operator *(float scale, Color value) => new((byte) (value.R * scale), (byte) (value.G * scale), (byte) (value.B * scale), (byte) (value.A * scale));

    /// <summary>
    /// Scales a color.
    /// </summary>
    /// <param name="value">The factor by which to scale the color.</param>
    /// <param name="scale">The color to scale.</param>
    /// <returns>The scaled color.</returns>
    public static Color operator *(Color value, float scale) => new((byte) (value.R * scale), (byte) (value.G * scale), (byte) (value.B * scale), (byte) (value.A * scale));

    /// <summary>
    /// Modulates two colors.
    /// </summary>
    /// <param name="left">The first color to modulate.</param>
    /// <param name="right">The second color to modulate.</param>
    /// <returns>The modulated color.</returns>
    public static Color operator *(Color left, Color right) => new((byte) (left.R * right.R / 255.0f), (byte) (left.G * right.G / 255.0f), (byte) (left.B * right.B / 255.0f), (byte) (left.A * right.A / 255.0f));

    /// <summary>
    /// Tests for equality between two objects.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
    [MethodImpl((MethodImplOptions) 0x100)] // MethodImplOptions.AggressiveInlining
    public static bool operator ==(Color left, Color right) => left.Equals(ref right);

    /// <summary>
    /// Tests for inequality between two objects.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
    [MethodImpl((MethodImplOptions) 0x100)] // MethodImplOptions.AggressiveInlining
    public static bool operator !=(Color left, Color right) => !left.Equals(ref right);

    /// <summary>
    /// Performs an explicit conversion from <see cref="System.Int32"/> to <see cref="Color"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static explicit operator int(Color value) => value.ToRgba();

    /// <summary>
    /// Performs an explicit conversion from <see cref="System.Int32"/> to <see cref="Color"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static explicit operator Color(int value) => new(value);

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override readonly string ToString() => ToString(CultureInfo.CurrentCulture);

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <param name="format">The format to apply to each channel element (byte).</param>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public readonly string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <param name="formatProvider">The format provider.</param>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public readonly string ToString(IFormatProvider? formatProvider) => string.Format(formatProvider, ToStringFormat, A, R, G, B);

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <param name="format">The format to apply to each channel element (byte).</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public readonly string ToString(string? format, IFormatProvider? formatProvider) {
        if (format == null) {
            return ToString(formatProvider);
        }

        return string.Format(formatProvider,
                             ToStringFormat,
                             A.ToString(format, formatProvider),
                             R.ToString(format, formatProvider),
                             G.ToString(format, formatProvider),
                             B.ToString(format, formatProvider));
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    public override readonly int GetHashCode() => HashCode.Combine(R, G, B, A);

    /// <summary>
    /// Determines whether the specified <see cref="Color"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="Color"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="Color"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl((MethodImplOptions) 0x100)] // MethodImplOptions.AggressiveInlining
    public readonly bool Equals(ref Color other) => R == other.R && G == other.G && B == other.B && A == other.A;

    /// <summary>
    /// Determines whether the specified <see cref="Color"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="Color"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="Color"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl((MethodImplOptions) 0x100)] // MethodImplOptions.AggressiveInlining
    public readonly bool Equals(Color other) => Equals(ref other);

    /// <summary>
    /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
    /// </summary>
    /// <param name="value">The <see cref="System.Object"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override readonly bool Equals(object? value) {
        if (value is not Color) {
            return false;
        }

        var strongValue = (Color) value;
        return Equals(ref strongValue);
    }

    private static byte ToByte(float component) {
        var value = (int) (component * 255.0f);
        return ToByte(value);
    }

    public static byte ToByte(int value) => (byte) (value < 0 ? 0 : value > 255 ? 255 : value);

}
