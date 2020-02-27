// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ReedSolomon.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Calculates a Reed-Solomon.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2020 Natalia Portillo
// Copyright (C) 1996 Phil Karn
// Copyright (C) 1995 Robert Morelos-Zaragoza
// Copyright (C) 1995 Hari Thirumoorthy
// ****************************************************************************/

/*
 * Reed-Solomon coding and decoding
 * Phil Karn (karn at ka9q.ampr.org) September 1996
 *
 * This file is derived from the program "new_rs_erasures.c" by Robert
 * Morelos-Zaragoza (robert at spectra.eng.hawaii.edu) and Hari Thirumoorthy
 * (harit at spectra.eng.hawaii.edu), Aug 1995
 *
 * I've made changes to improve performance, clean up the code and make it
 * easier to follow. Data is now passed to the encoding and decoding functions
 * through arguments rather than in global arrays. The decode function returns
 * the number of corrected symbols, or -1 if the word is uncorrectable.
 *
 * This code supports a symbol size from 2 bits up to 16 bits,
 * implying a block size of 3 2-bit symbols (6 bits) up to 65535
 * 16-bit symbols (1,048,560 bits). The code parameters are set in rs.h.
 *
 * Note that if symbols larger than 8 bits are used, the type of each
 * data array element switches from unsigned char to unsigned int. The
 * caller must ensure that elements larger than the symbol range are
 * not passed to the encoder or decoder.
 *
 */

using System;
using Aaru.Console;

namespace Aaru.Checksums
{
    /// <summary>
    ///     Implements the Reed-Solomon algorithm
    /// </summary>
    public class ReedSolomon
    {
        /// <summary>
        ///     Alpha exponent for the first root of the generator polynomial
        /// </summary>
        const int B0 = 1;
        /// <summary>
        ///     No legal value in index form represents zero, so we need a special value for this purpose
        /// </summary>
        int a0;
        /// <summary>
        ///     index->polynomial form conversion table
        /// </summary>
        int[] alpha_to;
        /// <summary>
        ///     Generator polynomial g(x) Degree of g(x) = 2*TT has roots @**B0, @**(B0+1), ... ,@^(B0+2*TT-1)
        /// </summary>
        int[] gg;
        /// <summary>
        ///     Polynomial->index form conversion table
        /// </summary>
        int[] index_of;
        bool initialized;
        int  mm, kk, nn;
        /// <summary>
        ///     Primitive polynomials - see Lin & Costello, Error Control Coding Appendix A, and  Lee & Messerschmitt, Digital
        ///     Communication p. 453.
        /// </summary>
        int[] pp;

        /// <summary>
        ///     Initializes the Reed-Solomon with RS(n,k) with GF(2^m)
        /// </summary>
        public void InitRs(int n, int k, int m)
        {
            switch(m)
            {
                case 2:
                    pp = new[] {1, 1, 1};
                    break;
                case 3:
                    pp = new[] {1, 1, 0, 1};
                    break;
                case 4:
                    pp = new[] {1, 1, 0, 0, 1};
                    break;
                case 5:
                    pp = new[] {1, 0, 1, 0, 0, 1};
                    break;
                case 6:
                    pp = new[] {1, 1, 0, 0, 0, 0, 1};
                    break;
                case 7:
                    pp = new[] {1, 0, 0, 1, 0, 0, 0, 1};
                    break;
                case 8:
                    pp = new[] {1, 0, 1, 1, 1, 0, 0, 0, 1};
                    break;
                case 9:
                    pp = new[] {1, 0, 0, 0, 1, 0, 0, 0, 0, 1};
                    break;
                case 10:
                    pp = new[] {1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1};
                    break;
                case 11:
                    pp = new[] {1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1};
                    break;
                case 12:
                    pp = new[] {1, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1};
                    break;
                case 13:
                    pp = new[] {1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1};
                    break;
                case 14:
                    pp = new[] {1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1};
                    break;
                case 15:
                    pp = new[] {1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1};
                    break;
                case 16:
                    pp = new[] {1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1};
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(m), "m must be between 2 and 16 inclusive");
            }

            mm       = m;
            kk       = k;
            nn       = n;
            a0       = n;
            alpha_to = new int[n + 1];
            index_of = new int[n + 1];

            gg = new int[nn - kk + 1];

            generate_gf();
            gen_poly();

            initialized = true;
        }

        int Modnn(int x)
        {
            while(x >= nn)
            {
                x -= nn;
                x =  (x >> mm) + (x & nn);
            }

            return x;
        }

        static int Min(int a, int b) => a < b ? a : b;

        static void Clear(ref int[] a, int n)
        {
            int ci;
            for(ci = n - 1; ci >= 0; ci--) a[ci] = 0;
        }

        static void Copy(ref int[] a, ref int[] b, int n)
        {
            int ci;
            for(ci = n - 1; ci >= 0; ci--) a[ci] = b[ci];
        }

        static void Copydown(ref int[] a, ref int[] b, int n)
        {
            int ci;
            for(ci = n - 1; ci >= 0; ci--) a[ci] = b[ci];
        }

        /* generate GF(2**m) from the irreducible polynomial p(X) in p[0]..p[m]
           lookup tables:  index->polynomial form   alpha_to[] contains j=alpha**i;
                           polynomial form -> index form  index_of[j=alpha**i] = i
           alpha=2 is the primitive element of GF(2**m)
           HARI's COMMENT: (4/13/94) alpha_to[] can be used as follows:
                Let @ represent the primitive element commonly called "alpha" that
           is the root of the primitive polynomial p(x). Then in GF(2^m), for any
           0 <= i <= 2^m-2,
                @^i = a(0) + a(1) @ + a(2) @^2 + ... + a(m-1) @^(m-1)
           where the binary vector (a(0),a(1),a(2),...,a(m-1)) is the representation
           of the integer "alpha_to[i]" with a(0) being the LSB and a(m-1) the MSB. Thus for
           example the polynomial representation of @^5 would be given by the binary
           representation of the integer "alpha_to[5]".
                           Similarily, index_of[] can be used as follows:
                As above, let @ represent the primitive element of GF(2^m) that is
           the root of the primitive polynomial p(x). In order to find the power
           of @ (alpha) that has the polynomial representation
                a(0) + a(1) @ + a(2) @^2 + ... + a(m-1) @^(m-1)
           we consider the integer "i" whose binary representation with a(0) being LSB
           and a(m-1) MSB is (a(0),a(1),...,a(m-1)) and locate the entry
           "index_of[i]". Now, @^index_of[i] is that element whose polynomial
            representation is (a(0),a(1),a(2),...,a(m-1)).
           NOTE:
                The element alpha_to[2^m-1] = 0 always signifying that the
           representation of "@^infinity" = 0 is (0,0,0,...,0).
                Similarily, the element index_of[0] = A0 always signifying
           that the power of alpha which has the polynomial representation
           (0,0,...,0) is "infinity".

        */
        void generate_gf()
        {
            int i;

            int mask = 1;
            alpha_to[mm] = 0;
            for(i = 0; i < mm; i++)
            {
                alpha_to[i]           = mask;
                index_of[alpha_to[i]] = i;
                /* If Pp[i] == 1 then, term @^i occurs in poly-repr of @^MM */
                if(pp[i] != 0) alpha_to[mm] ^= mask; /* Bit-wise EXOR operation */
                mask <<= 1;                          /* single left-shift */
            }

            index_of[alpha_to[mm]] = mm;
            /*
             * Have obtained poly-repr of @^MM. Poly-repr of @^(i+1) is given by
             * poly-repr of @^i shifted left one-bit and accounting for any @^MM
             * term that may occur when poly-repr of @^i is shifted.
             */
            mask >>= 1;
            for(i = mm + 1; i < nn; i++)
            {
                if(alpha_to[i - 1] >= mask) alpha_to[i] = alpha_to[mm] ^ ((alpha_to[i - 1] ^ mask) << 1);
                else alpha_to[i]                        = alpha_to[i - 1] << 1;
                index_of[alpha_to[i]] = i;
            }

            index_of[0]  = a0;
            alpha_to[nn] = 0;
        }

        /*
         * Obtain the generator polynomial of the TT-error correcting, length
         * NN=(2**MM -1) Reed Solomon code from the product of (X+@**(B0+i)), i = 0,
         * ... ,(2*TT-1)
         *
         * Examples:
         *
         * If B0 = 1, TT = 1. deg(g(x)) = 2*TT = 2.
         * g(x) = (x+@) (x+@**2)
         *
         * If B0 = 0, TT = 2. deg(g(x)) = 2*TT = 4.
         * g(x) = (x+1) (x+@) (x+@**2) (x+@**3)
         */
        void gen_poly()
        {
            int i;

            gg[0] = alpha_to[B0];
            gg[1] = 1; /* g(x) = (X+@**B0) initially */
            for(i = 2; i <= nn - kk; i++)
            {
                gg[i] = 1;
                /*
                 * Below multiply (Gg[0]+Gg[1]*x + ... +Gg[i]x^i) by
                 * (@**(B0+i-1) + x)
                 */
                for(int j = i - 1; j > 0; j--)
                    if(gg[j] != 0)
                        gg[j] = gg[j - 1] ^ alpha_to[Modnn(index_of[gg[j]] + B0 + i - 1)];
                    else
                        gg[j] = gg[j - 1];
                /* Gg[0] can never be zero */
                gg[0] = alpha_to[Modnn(index_of[gg[0]] + B0 + i - 1)];
            }

            /* convert Gg[] to index form for quicker encoding */
            for(i = 0; i <= nn - kk; i++) gg[i] = index_of[gg[i]];
        }

        /*
         * take the string of symbols in data[i], i=0..(k-1) and encode
         * systematically to produce NN-KK parity symbols in bb[0]..bb[NN-KK-1] data[]
         * is input and bb[] is output in polynomial form. Encoding is done by using
         * a feedback shift register with appropriate connections specified by the
         * elements of Gg[], which was generated above. Codeword is   c(X) =
         * data(X)*X**(NN-KK)+ b(X)
         */
        /// <summary>
        ///     Takes the symbols in data to output parity in bb.
        /// </summary>
        /// <returns>Returns -1 if an illegal symbol is found.</returns>
        /// <param name="data">Data symbols.</param>
        /// <param name="bb">Outs parity symbols.</param>
        public int encode_rs(int[] data, out int[] bb)
        {
            if(!initialized) throw new UnauthorizedAccessException("Trying to calculate RS without initializing!");

            int i;
            bb = new int[nn - kk];

            Clear(ref bb, nn - kk);
            for(i = kk - 1; i >= 0; i--)
            {
                if(mm != 8)
                    if(data[i] > nn)
                        return -1; /* Illegal symbol */

                int feedback = index_of[data[i] ^ bb[nn - kk - 1]];
                if(feedback != a0)
                {
                    /* feedback term is non-zero */
                    for(int j = nn - kk - 1; j > 0; j--)
                        if(gg[j] != a0)
                            bb[j] = bb[j - 1] ^ alpha_to[Modnn(gg[j] + feedback)];
                        else
                            bb[j] = bb[j - 1];

                    bb[0] = alpha_to[Modnn(gg[0] + feedback)];
                }
                else
                {
                    /* feedback term is zero. encoder becomes a
                                     * single-byte shifter */
                    for(int j = nn - kk - 1; j > 0; j--) bb[j] = bb[j - 1];

                    bb[0] = 0;
                }
            }

            return 0;
        }

        /*
         * Performs ERRORS+ERASURES decoding of RS codes. If decoding is successful,
         * writes the codeword into data[] itself. Otherwise data[] is unaltered.
         *
         * Return number of symbols corrected, or -1 if codeword is illegal
         * or uncorrectable.
         *
         * First "no_eras" erasures are declared by the calling program. Then, the
         * maximum # of errors correctable is t_after_eras = floor((NN-KK-no_eras)/2).
         * If the number of channel errors is not greater than "t_after_eras" the
         * transmitted codeword will be recovered. Details of algorithm can be found
         * in R. Blahut's "Theory ... of Error-Correcting Codes".
         */
        /// <summary>
        ///     Decodes the RS. If decoding is successful outputs corrected data symbols.
        /// </summary>
        /// <returns>Returns corrected symbols, -1 if illegal or uncorrectable</returns>
        /// <param name="data">Data symbols.</param>
        /// <param name="erasPos">Position of erasures.</param>
        /// <param name="noEras">Number of erasures.</param>
        public int eras_dec_rs(ref int[] data, out int[] erasPos, int noEras)
        {
            if(!initialized) throw new UnauthorizedAccessException("Trying to calculate RS without initializing!");

            erasPos = new int[nn - kk];
            int   i, j;
            int   q, tmp;
            int[] recd   = new int[nn];
            int[] lambda = new int[nn - kk + 1]; /* Err+Eras Locator poly */
            int[] s      = new int[nn - kk + 1]; /* syndrome poly */
            int[] b      = new int[nn - kk + 1];
            int[] t      = new int[nn - kk + 1];
            int[] omega  = new int[nn - kk + 1];
            int[] root   = new int[nn      - kk];
            int[] reg    = new int[nn - kk + 1];
            int[] loc    = new int[nn      - kk];
            int   count;

            /* data[] is in polynomial form, copy and convert to index form */
            for(i = nn - 1; i >= 0; i--)
            {
                if(mm != 8)
                    if(data[i] > nn)
                        return -1; /* Illegal symbol */

                recd[i] = index_of[data[i]];
            }

            /* first form the syndromes; i.e., evaluate recd(x) at roots of g(x)
             * namely @**(B0+i), i = 0, ... ,(NN-KK-1)
             */
            int synError = 0;
            for(i = 1; i <= nn - kk; i++)
            {
                tmp = 0;
                for(j = 0; j < nn; j++)
                    if(recd[j] != a0) /* recd[j] in index form */
                        tmp ^= alpha_to[Modnn(recd[j] + (B0 + i - 1) * j)];

                synError |= tmp; /* set flag if non-zero syndrome =>
                     * error */
                /* store syndrome in index form  */
                s[i] = index_of[tmp];
            }

            if(synError == 0) return 0;

            Clear(ref lambda, nn - kk);
            lambda[0] = 1;
            if(noEras > 0)
            {
                /* Init lambda to be the erasure locator polynomial */
                lambda[1] = alpha_to[erasPos[0]];
                for(i = 1; i < noEras; i++)
                {
                    int u = erasPos[i];
                    for(j = i + 1; j > 0; j--)
                    {
                        tmp = index_of[lambda[j - 1]];
                        if(tmp != a0) lambda[j] ^= alpha_to[Modnn(u + tmp)];
                    }
                }

                #if DEBUG
                /* find roots of the erasure location polynomial */
                for(i = 1; i <= noEras; i++) reg[i] = index_of[lambda[i]];

                count = 0;
                for(i = 1; i <= nn; i++)
                {
                    q = 1;
                    for(j = 1; j <= noEras; j++)
                        if(reg[j] != a0)
                        {
                            reg[j] =  Modnn(reg[j] + j);
                            q      ^= alpha_to[reg[j]];
                        }

                    if(q != 0) continue;

                    /* store root and error location
                             * number indices
                             */
                    root[count] = i;
                    loc[count]  = nn - i;
                    count++;
                }

                if(count != noEras)
                {
                    DicConsole.DebugWriteLine("Reed Solomon", "\n lambda(x) is WRONG\n");
                    return -1;
                }

                DicConsole.DebugWriteLine("Reed Solomon",
                                          "\n Erasure positions as determined by roots of Eras Loc Poly:\n");
                for(i = 0; i < count; i++) DicConsole.DebugWriteLine("Reed Solomon", "{0} ", loc[i]);

                DicConsole.DebugWriteLine("Reed Solomon", "\n");
                #endif
            }

            for(i = 0; i < nn - kk + 1; i++) b[i] = index_of[lambda[i]];

            /*
             * Begin Berlekamp-Massey algorithm to determine error+erasure
             * locator polynomial
             */
            int r  = noEras;
            int el = noEras;
            while(++r <= nn - kk)
            {
                /* r is the step number */
                /* Compute discrepancy at the r-th step in poly-form */
                int discrR = 0;
                for(i = 0; i < r; i++)
                    if(lambda[i] != 0 && s[r - i] != a0)
                        discrR ^= alpha_to[Modnn(index_of[lambda[i]] + s[r - i])];

                discrR = index_of[discrR]; /* Index form */
                if(discrR == a0)
                {
                    /* 2 lines below: B(x) <-- x*B(x) */
                    Copydown(ref b, ref b, nn - kk);
                    b[0] = a0;
                }
                else
                {
                    /* 7 lines below: T(x) <-- lambda(x) - discr_r*x*b(x) */
                    t[0] = lambda[0];
                    for(i = 0; i < nn - kk; i++)
                        if(b[i] != a0)
                            t[i + 1] = lambda[i + 1] ^ alpha_to[Modnn(discrR + b[i])];
                        else
                            t[i + 1] = lambda[i + 1];

                    if(2 * el <= r + noEras - 1)
                    {
                        el = r + noEras - el;
                        /*
                         * 2 lines below: B(x) <-- inv(discr_r) *
                         * lambda(x)
                         */
                        for(i = 0; i <= nn - kk; i++)
                            b[i] = lambda[i] == 0 ? a0 : Modnn(index_of[lambda[i]] - discrR + nn);
                    }
                    else
                    {
                        /* 2 lines below: B(x) <-- x*B(x) */
                        Copydown(ref b, ref b, nn - kk);
                        b[0] = a0;
                    }

                    Copy(ref lambda, ref t, nn - kk + 1);
                }
            }

            /* Convert lambda to index form and compute deg(lambda(x)) */
            int degLambda = 0;
            for(i = 0; i < nn - kk + 1; i++)
            {
                lambda[i] = index_of[lambda[i]];
                if(lambda[i] != a0) degLambda = i;
            }

            /*
             * Find roots of the error+erasure locator polynomial. By Chien
             * Search
             */
            int temp = reg[0];
            Copy(ref reg, ref lambda, nn - kk);
            reg[0] = temp;
            count  = 0; /* Number of roots of lambda(x) */
            for(i = 1; i <= nn; i++)
            {
                q = 1;
                for(j = degLambda; j > 0; j--)
                    if(reg[j] != a0)
                    {
                        reg[j] =  Modnn(reg[j] + j);
                        q      ^= alpha_to[reg[j]];
                    }

                if(q != 0) continue;

                /* store root (index-form) and error location number */
                root[count] = i;
                loc[count]  = nn - i;
                count++;
            }

            #if DEBUG
            DicConsole.DebugWriteLine("Reed Solomon", "\n Final error positions:\t");
            for(i = 0; i < count; i++) DicConsole.DebugWriteLine("Reed Solomon", "{0} ", loc[i]);

            DicConsole.DebugWriteLine("Reed Solomon", "\n");
            #endif

            if(degLambda != count) return -1;

            /*
             * Compute err+eras evaluator poly omega(x) = s(x)*lambda(x) (modulo
             * x**(NN-KK)). in index form. Also find deg(omega).
             */
            int degOmega = 0;
            for(i = 0; i < nn - kk; i++)
            {
                tmp = 0;
                j   = degLambda < i ? degLambda : i;
                for(; j >= 0; j--)
                    if(s[i + 1 - j] != a0 && lambda[j] != a0)
                        tmp ^= alpha_to[Modnn(s[i + 1 - j] + lambda[j])];

                if(tmp != 0) degOmega = i;
                omega[i] = index_of[tmp];
            }

            omega[nn - kk] = a0;

            /*
             * Compute error values in poly-form. num1 = omega(inv(X(l))), num2 =
             * inv(X(l))**(B0-1) and den = lambda_pr(inv(X(l))) all in poly-form
             */
            for(j = count - 1; j >= 0; j--)
            {
                int num1 = 0;
                for(i = degOmega; i >= 0; i--)
                    if(omega[i] != a0)
                        num1 ^= alpha_to[Modnn(omega[i] + i * root[j])];

                int num2 = alpha_to[Modnn(root[j] * (B0 - 1) + nn)];
                int den  = 0;

                /* lambda[i+1] for i even is the formal derivative lambda_pr of lambda[i] */
                for(i = Min(degLambda, nn - kk - 1) & ~1; i >= 0; i -= 2)
                    if(lambda[i + 1] != a0)
                        den ^= alpha_to[Modnn(lambda[i + 1] + i * root[j])];

                if(den == 0)
                {
                    DicConsole.DebugWriteLine("Reed Solomon", "\n ERROR: denominator = 0\n");
                    return -1;
                }

                /* Apply error to data */
                if(num1 != 0) data[loc[j]] ^= alpha_to[Modnn(index_of[num1] + index_of[num2] + nn - index_of[den])];
            }

            return count;
        }
    }
}