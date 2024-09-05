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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Imcodec.Math;

/// <summary>
/// Structure using the same layout than <see cref="System.Drawing.Point"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Point"/> struct.
/// </remarks>
/// <param name="x">The x.</param>
/// <param name="y">The y.</param>
[StructLayout(LayoutKind.Sequential)]
public struct Point(int x, int y) : IEquatable<Point> {

    /// <summary>
    /// A point with (0,0) coordinates.
    /// </summary>
    public static readonly Point Zero = new(0, 0);

    /// <summary>
    /// Left coordinate.
    /// </summary>
    public int X = x;

    /// <summary>
    /// Top coordinate.
    /// </summary>
    public int Y = y;

    /// <summary>
    /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="System.Object"/> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl((MethodImplOptions) 0x100)] // MethodImplOptions.AggressiveInlining
    public readonly bool Equals(ref Point other) => other.X == X && other.Y == Y;

    /// <summary>
    /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="System.Object"/> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl((MethodImplOptions) 0x100)] // MethodImplOptions.AggressiveInlining
    public readonly bool Equals(Point other) => Equals(ref other);

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj) {
        if (obj is not Point) {
            return false;
        }

        var strongValue = (Point) obj;
        return Equals(ref strongValue);
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode() {
        unchecked {
            return (X * 397) ^ Y;
        }
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    [MethodImpl((MethodImplOptions) 0x100)] // MethodImplOptions.AggressiveInlining
    public static bool operator ==(Point left, Point right) => left.Equals(ref right);

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    [MethodImpl((MethodImplOptions) 0x100)] // MethodImplOptions.AggressiveInlining
    public static bool operator !=(Point left, Point right) => !left.Equals(ref right);

    public override readonly string ToString() => string.Format("({0},{1})", X, Y);

    /// <summary>
    /// Performs an explicit conversion from <see cref="Vector2"/> to <see cref="Point"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator Point(Vector2 value) => new((int) value.X, (int) value.Y);

    /// <summary>
    /// Performs an implicit conversion from <see cref="Point"/> to <see cref="Vector2"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Vector2(Point value) => new(value.X, value.Y);

}
