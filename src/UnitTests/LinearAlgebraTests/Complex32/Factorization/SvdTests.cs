﻿// <copyright file="SvdTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.Factorization
{
    using System;
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Complex32.Factorization;
    using LinearAlgebra.Generic.Factorization;
    using NUnit.Framework;
    using Complex32 = Numerics.Complex32;

    /// <summary>
    /// Svd factorization tests for a dense matrix.
    /// </summary>
    public class SvdTests
    {
        /// <summary>
        /// Constructor with <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DenseSvd(null, true));
        }

        /// <summary>
        /// Can factorize identity matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void CanFactorizeIdentity([Values(1, 10, 100)] int order)
        {
            var matrixI = DenseMatrix.Identity(order);
            var factorSvd = matrixI.Svd(true);

            Assert.AreEqual(matrixI.RowCount, factorSvd.U().RowCount);
            Assert.AreEqual(matrixI.RowCount, factorSvd.U().ColumnCount);

            Assert.AreEqual(matrixI.ColumnCount, factorSvd.VT().RowCount);
            Assert.AreEqual(matrixI.ColumnCount, factorSvd.VT().ColumnCount);

            Assert.AreEqual(matrixI.RowCount, factorSvd.W().RowCount);
            Assert.AreEqual(matrixI.ColumnCount, factorSvd.W().ColumnCount);

            for (var i = 0; i < factorSvd.W().RowCount; i++)
            {
                for (var j = 0; j < factorSvd.W().ColumnCount; j++)
                {
                    Assert.AreEqual(i == j ? Complex32.One : Complex32.Zero, factorSvd.W()[i, j]);
                }
            }
        }

        /// <summary>
        /// Can factorize a random matrix.
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [Test, Sequential]
        public void CanFactorizeRandomMatrix([Values(1, 2, 5, 10, 50, 100)] int row, [Values(1, 2, 5, 6, 48, 98)] int column)
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(row, column);
            var factorSvd = matrixA.Svd(true);

            // Make sure the U has the right dimensions.
            Assert.AreEqual(row, factorSvd.U().RowCount);
            Assert.AreEqual(row, factorSvd.U().ColumnCount);

            // Make sure the VT has the right dimensions.
            Assert.AreEqual(column, factorSvd.VT().RowCount);
            Assert.AreEqual(column, factorSvd.VT().ColumnCount);

            // Make sure the W has the right dimensions.
            Assert.AreEqual(row, factorSvd.W().RowCount);
            Assert.AreEqual(column, factorSvd.W().ColumnCount);

            // Make sure the U*W*VT is the original matrix.
            var matrix = factorSvd.U() * factorSvd.W() * factorSvd.VT();
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixA[i, j].Real, matrix[i, j].Real, 1e-3f);
                    Assert.AreEqual(matrixA[i, j].Imaginary, matrix[i, j].Imaginary, 1e-3f);
                }
            }
        }

        /// <summary>
        /// Can check rank of a non-square matrix.
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [Test, Sequential]
        public void CanCheckRankOfNonSquare([Values(10, 48, 100)] int row, [Values(8, 52, 93)] int column)
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(row, column);
            var factorSvd = matrixA.Svd(true);

            var mn = Math.Min(row, column);
            Assert.AreEqual(factorSvd.Rank, mn);
        }

        /// <summary>
        /// Can check rank of a square matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void CanCheckRankSquare([Values(1, 2, 5, 9, 50, 90)] int order)
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(order, order);
            var factorSvd = matrixA.Svd(true);

            if (factorSvd.Determinant != 0)
            {
                Assert.AreEqual(factorSvd.Rank, order);
            }
            else
            {
                Assert.AreEqual(factorSvd.Rank, order - 1);
            }
        }

        /// <summary>
        /// Can check rank of a square singular matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void CanCheckRankOfSquareSingular([Values(10, 50, 100)] int order)
        {
            var matrixA = new DenseMatrix(order, order);
            matrixA[0, 0] = 1;
            matrixA[order - 1, order - 1] = 1;
            for (var i = 1; i < order - 1; i++)
            {
                matrixA[i, i - 1] = 1;
                matrixA[i, i + 1] = 1;
                matrixA[i - 1, i] = 1;
                matrixA[i + 1, i] = 1;
            }

            var factorSvd = matrixA.Svd(true);

            Assert.AreEqual(factorSvd.Determinant, Complex32.Zero);
            Assert.AreEqual(factorSvd.Rank, order - 1);
        }

        /// <summary>
        /// Solve for matrix if vectors are not computed throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveMatrixIfVectorsNotComputedThrowsInvalidOperationException()
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(10, 3);
            var factorSvd = matrixA.Svd(false);

            var matrixB = MatrixLoader.GenerateRandomDenseMatrix(10, 3);
            Assert.Throws<InvalidOperationException>(() => factorSvd.Solve(matrixB));
        }

        /// <summary>
        /// Solve for vector if vectors are not computed throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveVectorIfVectorsNotComputedThrowsInvalidOperationException()
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(10, 3);
            var factorSvd = matrixA.Svd(false);

            var vectorb = MatrixLoader.GenerateRandomDenseVector(3);
            Assert.Throws<InvalidOperationException>(() => factorSvd.Solve(vectorb));
        }

        /// <summary>
        /// Can solve a system of linear equations for a random vector (Ax=b).
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [Test, Sequential]
        public void CanSolveForRandomVector([Values(1, 2, 5, 9, 50, 90)] int row, [Values(1, 2, 5, 10, 50, 100)] int column)
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(row, column);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);

            var vectorb = MatrixLoader.GenerateRandomDenseVector(row);
            var resultx = factorSvd.Solve(vectorb);

            Assert.AreEqual(matrixA.ColumnCount, resultx.Count);

            var matrixBReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreEqual(vectorb[i].Real, matrixBReconstruct[i].Real, 1e-3f);
                Assert.AreEqual(vectorb[i].Imaginary, matrixBReconstruct[i].Imaginary, 1e-3f);
            }

            // Make sure A didn't change.
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixACopy[i, j], matrixA[i, j]);
                }
            }
        }

        /// <summary>
        /// Can solve a system of linear equations for a random matrix (AX=B).
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [Test, Sequential]
        public void CanSolveForRandomMatrix([Values(1, 4, 7, 10, 45, 80)] int row, [Values(1, 4, 8, 10, 50, 100)] int column)
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(row, column);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);

            var matrixB = MatrixLoader.GenerateRandomDenseMatrix(row, column);
            var matrixX = factorSvd.Solve(matrixB);

            // The solution X row dimension is equal to the column dimension of A
            Assert.AreEqual(matrixA.ColumnCount, matrixX.RowCount);

            // The solution X has the same number of columns as B
            Assert.AreEqual(matrixB.ColumnCount, matrixX.ColumnCount);

            var matrixBReconstruct = matrixA * matrixX;

            // Check the reconstruction.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixB[i, j].Real, matrixBReconstruct[i, j].Real, 1e-3f);
                    Assert.AreEqual(matrixB[i, j].Imaginary, matrixBReconstruct[i, j].Imaginary, 1e-3f);
                }
            }

            // Make sure A didn't change.
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixACopy[i, j], matrixA[i, j]);
                }
            }
        }

        /// <summary>
        /// Can solve for a random vector into a result vector.
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [Test, Sequential]
        public void CanSolveForRandomVectorWhenResultVectorGiven([Values(1, 2, 5, 9, 50, 90)] int row, [Values(1, 2, 5, 10, 50, 100)] int column)
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(row, column);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);
            var vectorb = MatrixLoader.GenerateRandomDenseVector(row);
            var vectorbCopy = vectorb.Clone();
            var resultx = new DenseVector(column);
            factorSvd.Solve(vectorb, resultx);

            var matrixBReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreEqual(vectorb[i].Real, matrixBReconstruct[i].Real, 1e-3f);
                Assert.AreEqual(vectorb[i].Imaginary, matrixBReconstruct[i].Imaginary, 1e-3f);
            }

            // Make sure A didn't change.
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixACopy[i, j], matrixA[i, j]);
                }
            }

            // Make sure b didn't change.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreEqual(vectorbCopy[i], vectorb[i]);
            }
        }

        /// <summary>
        /// Can solve a system of linear equations for a random matrix (AX=B) into a result matrix.
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [Test, Sequential]
        public void CanSolveForRandomMatrixWhenResultMatrixGiven([Values(1, 4, 7, 10, 45, 80)] int row, [Values(1, 4, 8, 10, 50, 100)] int column)
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(row, column);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);

            var matrixB = MatrixLoader.GenerateRandomDenseMatrix(row, column);
            var matrixBCopy = matrixB.Clone();

            var matrixX = new DenseMatrix(column, column);
            factorSvd.Solve(matrixB, matrixX);

            // The solution X row dimension is equal to the column dimension of A
            Assert.AreEqual(matrixA.ColumnCount, matrixX.RowCount);

            // The solution X has the same number of columns as B
            Assert.AreEqual(matrixB.ColumnCount, matrixX.ColumnCount);

            var matrixBReconstruct = matrixA * matrixX;

            // Check the reconstruction.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixB[i, j].Real, matrixBReconstruct[i, j].Real, 1e-3f);
                    Assert.AreEqual(matrixB[i, j].Imaginary, matrixBReconstruct[i, j].Imaginary, 1e-3f);
                }
            }

            // Make sure A didn't change.
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixACopy[i, j], matrixA[i, j]);
                }
            }

            // Make sure B didn't change.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixBCopy[i, j], matrixB[i, j]);
                }
            }
        }
    }
}
