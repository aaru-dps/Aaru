// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2016 Natalia Portillo
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
using DiscImageChef.Console;

namespace DiscImageChef.Checksums
{
    public class ReedSolomon
    {
        /* Primitive polynomials - see Lin & Costello, Error Control Coding Appendix A,
         * and  Lee & Messerschmitt, Digital Communication p. 453.
         */
        int[] Pp;
        /* index->polynomial form conversion table */
        int[] Alpha_to;
        /* Polynomial->index form conversion table */
        int[] Index_of;
        /* Generator polynomial g(x)
         * Degree of g(x) = 2*TT
         * has roots @**B0, @**(B0+1), ... ,@^(B0+2*TT-1)
         */
        int[] Gg;
        int MM, KK, NN;
        /* No legal value in index form represents zero, so
         * we need a special value for this purpose
         */
        int A0;
        bool initialized;
        /* Alpha exponent for the first root of the generator polynomial */
        const int B0 = 1;

        /// <summary>
        /// Initializes the Reed-Solomon with RS(n,k) with GF(2^m)
        /// </summary>
        public void InitRS(int n, int k, int m)
        {
            switch(m)
            {
                case 2:
                    Pp = new[] { 1, 1, 1 };
                    break;
                case 3:
                    Pp = new[] { 1, 1, 0, 1 };
                    break;
                case 4:
                    Pp = new[] { 1, 1, 0, 0, 1 };
                    break;
                case 5:
                    Pp = new[] { 1, 0, 1, 0, 0, 1 };
                    break;
                case 6:
                    Pp = new[] { 1, 1, 0, 0, 0, 0, 1 };
                    break;
                case 7:
                    Pp = new[] { 1, 0, 0, 1, 0, 0, 0, 1 };
                    break;
                case 8:
                    Pp = new[] { 1, 0, 1, 1, 1, 0, 0, 0, 1 };
                    break;
                case 9:
                    Pp = new[] { 1, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
                    break;
                case 10:
                    Pp = new[] { 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1 };
                    break;
                case 11:
                    Pp = new[] { 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
                    break;
                case 12:
                    Pp = new[] { 1, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1 };
                    break;
                case 13:
                    Pp = new[] { 1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
                    break;
                case 14:
                    Pp = new[] { 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 };
                    break;
                case 15:
                    Pp = new[] { 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
                    break;
                case 16:
                    Pp = new[] { 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1 };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(m), "m must be between 2 and 16 inclusive");
            }

            MM = m;
            KK = k;
            NN = n;
            A0 = n;
            Alpha_to = new int[n + 1];
            Index_of = new int[n + 1];


            Gg = new int[NN - KK + 1];

            generate_gf();
            gen_poly();

            initialized = true;
        }

        int modnn(int x)
        {
            while(x >= NN)
            {
                x -= NN;
                x = (x >> MM) + (x & NN);
            }
            return x;
        }

        static int min(int a, int b)
        {
            return ((a) < (b) ? (a) : (b));
        }

        static void CLEAR(ref int[] a, int n)
        {
            int ci;
            for(ci = (n) - 1; ci >= 0; ci--)
                (a)[ci] = 0;
        }

        static void COPY(ref int[] a, ref int[] b, int n)
        {
            int ci;
            for(ci = (n) - 1; ci >= 0; ci--)
                (a)[ci] = (b)[ci];
        }

        static void COPYDOWN(ref int[] a, ref int[] b, int n)
        {
            int ci;
            for(ci = (n) - 1; ci >= 0; ci--)
                (a)[ci] = (b)[ci];
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
            int i, mask;

            mask = 1;
            Alpha_to[MM] = 0;
            for(i = 0; i < MM; i++)
            {
                Alpha_to[i] = mask;
                Index_of[Alpha_to[i]] = i;
                /* If Pp[i] == 1 then, term @^i occurs in poly-repr of @^MM */
                if(Pp[i] != 0)
                    Alpha_to[MM] ^= mask;   /* Bit-wise EXOR operation */
                mask <<= 1; /* single left-shift */
            }
            Index_of[Alpha_to[MM]] = MM;
            /*
             * Have obtained poly-repr of @^MM. Poly-repr of @^(i+1) is given by
             * poly-repr of @^i shifted left one-bit and accounting for any @^MM
             * term that may occur when poly-repr of @^i is shifted.
             */
            mask >>= 1;
            for(i = MM + 1; i < NN; i++)
            {
                if(Alpha_to[i - 1] >= mask)
                    Alpha_to[i] = Alpha_to[MM] ^ ((Alpha_to[i - 1] ^ mask) << 1);
                else
                    Alpha_to[i] = Alpha_to[i - 1] << 1;
                Index_of[Alpha_to[i]] = i;
            }
            Index_of[0] = A0;
            Alpha_to[NN] = 0;
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
            int i, j;

            Gg[0] = Alpha_to[B0];
            Gg[1] = 1;      /* g(x) = (X+@**B0) initially */
            for(i = 2; i <= NN - KK; i++)
            {
                Gg[i] = 1;
                /*
                 * Below multiply (Gg[0]+Gg[1]*x + ... +Gg[i]x^i) by
                 * (@**(B0+i-1) + x)
                 */
                for(j = i - 1; j > 0; j--)
                    if(Gg[j] != 0)
                        Gg[j] = Gg[j - 1] ^ Alpha_to[modnn((Index_of[Gg[j]]) + B0 + i - 1)];
                    else
                        Gg[j] = Gg[j - 1];
                /* Gg[0] can never be zero */
                Gg[0] = Alpha_to[modnn((Index_of[Gg[0]]) + B0 + i - 1)];
            }
            /* convert Gg[] to index form for quicker encoding */
            for(i = 0; i <= NN - KK; i++)
                Gg[i] = Index_of[Gg[i]];
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
        /// Takes the symbols in data to output parity in bb.
        /// </summary>
        /// <returns>Returns -1 if an illegal symbol is found.</returns>
        /// <param name="data">Data symbols.</param>
        /// <param name="bb">Outs parity symbols.</param>
        public int encode_rs(int[] data, out int[] bb)
        {
            if(initialized)
            {
                int i, j;
                int feedback;
                bb = new int[NN - KK];

                CLEAR(ref bb, NN - KK);
                for(i = KK - 1; i >= 0; i--)
                {
                    if(MM != 8)
                    {
                        if(data[i] > NN)
                            return -1;  /* Illegal symbol */
                    }
                    feedback = Index_of[data[i] ^ bb[NN - KK - 1]];
                    if(feedback != A0)
                    {   /* feedback term is non-zero */
                        for(j = NN - KK - 1; j > 0; j--)
                            if(Gg[j] != A0)
                                bb[j] = bb[j - 1] ^ Alpha_to[modnn(Gg[j] + feedback)];
                            else
                                bb[j] = bb[j - 1];
                        bb[0] = Alpha_to[modnn(Gg[0] + feedback)];
                    }
                    else
                    {    /* feedback term is zero. encoder becomes a
                 * single-byte shifter */
                        for(j = NN - KK - 1; j > 0; j--)
                            bb[j] = bb[j - 1];
                        bb[0] = 0;
                    }
                }
                return 0;
            }
            throw new UnauthorizedAccessException("Trying to calculate RS without initializing!");
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
        /// Decodes the RS. If decoding is successful outputs corrected data symbols.
        /// </summary>
        /// <returns>Returns corrected symbols, -1 if illegal or uncorrectable</returns>
        /// <param name="data">Data symbols.</param>
        /// <param name="eras_pos">Position of erasures.</param>
        /// <param name="no_eras">Number of erasures.</param>
        public int eras_dec_rs(ref int[] data, out int[] eras_pos, int no_eras)
        {
            if(initialized)
            {
                eras_pos = new int[NN - KK];
                int deg_lambda, el, deg_omega;
                int i, j, r;
                int u, q, tmp, num1, num2, den, discr_r;
                int[] recd = new int[NN];
                int[] lambda = new int[NN - KK + 1]; /* Err+Eras Locator poly */
                int[] s = new int[NN - KK + 1]; /* syndrome poly */
                int[] b = new int[NN - KK + 1];
                int[] t = new int[NN - KK + 1];
                int[] omega = new int[NN - KK + 1];
                int[] root = new int[NN - KK];
                int[] reg = new int[NN - KK + 1];
                int[] loc = new int[NN - KK];
                int syn_error, count;

                /* data[] is in polynomial form, copy and convert to index form */
                for(i = NN - 1; i >= 0; i--)
                {
                    if(MM != 8)
                    {
                        if(data[i] > NN)
                            return -1;  /* Illegal symbol */
                    }
                    recd[i] = Index_of[data[i]];
                }
                /* first form the syndromes; i.e., evaluate recd(x) at roots of g(x)
             * namely @**(B0+i), i = 0, ... ,(NN-KK-1)
             */
                syn_error = 0;
                for(i = 1; i <= NN - KK; i++)
                {
                    tmp = 0;
                    for(j = 0; j < NN; j++)
                        if(recd[j] != A0)  /* recd[j] in index form */
                            tmp ^= Alpha_to[modnn(recd[j] + (B0 + i - 1) * j)];
                    syn_error |= tmp;   /* set flag if non-zero syndrome =>
                     * error */
                    /* store syndrome in index form  */
                    s[i] = Index_of[tmp];
                }
                if(syn_error == 0)
                {
                    /*
                 * if syndrome is zero, data[] is a codeword and there are no
                 * errors to correct. So return data[] unmodified
                 */
                    return 0;
                }
                CLEAR(ref lambda, NN - KK);
                lambda[0] = 1;
                if(no_eras > 0)
                {
                    /* Init lambda to be the erasure locator polynomial */
                    lambda[1] = Alpha_to[eras_pos[0]];
                    for(i = 1; i < no_eras; i++)
                    {
                        u = eras_pos[i];
                        for(j = i + 1; j > 0; j--)
                        {
                            tmp = Index_of[lambda[j - 1]];
                            if(tmp != A0)
                                lambda[j] ^= Alpha_to[modnn(u + tmp)];
                        }
                    }

#if DEBUG
                    /* find roots of the erasure location polynomial */
                    for(i = 1; i <= no_eras; i++)
                        reg[i] = Index_of[lambda[i]];
                    count = 0;
                    for(i = 1; i <= NN; i++)
                    {
                        q = 1;
                        for(j = 1; j <= no_eras; j++)
                            if(reg[j] != A0)
                            {
                                reg[j] = modnn(reg[j] + j);
                                q ^= Alpha_to[reg[j]];
                            }
                        if(q == 0)
                        {
                            /* store root and error location
                             * number indices
                             */
                            root[count] = i;
                            loc[count] = NN - i;
                            count++;
                        }
                    }
                    if(count != no_eras)
                    {
                        DicConsole.DebugWriteLine("Reed Solomon", "\n lambda(x) is WRONG\n");
                        return -1;
                    }

                    DicConsole.DebugWriteLine("Reed Solomon", "\n Erasure positions as determined by roots of Eras Loc Poly:\n");
                    for(i = 0; i < count; i++)
                        DicConsole.DebugWriteLine("Reed Solomon", "{0} ", loc[i]);
                    DicConsole.DebugWriteLine("Reed Solomon", "\n");
#endif
                }
                for(i = 0; i < NN - KK + 1; i++)
                    b[i] = Index_of[lambda[i]];

                /*
             * Begin Berlekamp-Massey algorithm to determine error+erasure
             * locator polynomial
             */
                r = no_eras;
                el = no_eras;
                while(++r <= NN - KK)
                {  /* r is the step number */
                    /* Compute discrepancy at the r-th step in poly-form */
                    discr_r = 0;
                    for(i = 0; i < r; i++)
                    {
                        if((lambda[i] != 0) && (s[r - i] != A0))
                        {
                            discr_r ^= Alpha_to[modnn(Index_of[lambda[i]] + s[r - i])];
                        }
                    }
                    discr_r = Index_of[discr_r];    /* Index form */
                    if(discr_r == A0)
                    {
                        /* 2 lines below: B(x) <-- x*B(x) */
                        COPYDOWN(ref b, ref b, NN - KK);
                        b[0] = A0;
                    }
                    else
                    {
                        /* 7 lines below: T(x) <-- lambda(x) - discr_r*x*b(x) */
                        t[0] = lambda[0];
                        for(i = 0; i < NN - KK; i++)
                        {
                            if(b[i] != A0)
                                t[i + 1] = lambda[i + 1] ^ Alpha_to[modnn(discr_r + b[i])];
                            else
                                t[i + 1] = lambda[i + 1];
                        }
                        if(2 * el <= r + no_eras - 1)
                        {
                            el = r + no_eras - el;
                            /*
                         * 2 lines below: B(x) <-- inv(discr_r) *
                         * lambda(x)
                         */
                            for(i = 0; i <= NN - KK; i++)
                                b[i] = (lambda[i] == 0) ? A0 : modnn(Index_of[lambda[i]] - discr_r + NN);
                        }
                        else
                        {
                            /* 2 lines below: B(x) <-- x*B(x) */
                            COPYDOWN(ref b, ref b, NN - KK);
                            b[0] = A0;
                        }
                        COPY(ref lambda, ref t, NN - KK + 1);
                    }
                }

                /* Convert lambda to index form and compute deg(lambda(x)) */
                deg_lambda = 0;
                for(i = 0; i < NN - KK + 1; i++)
                {
                    lambda[i] = Index_of[lambda[i]];
                    if(lambda[i] != A0)
                        deg_lambda = i;
                }
                /*
             * Find roots of the error+erasure locator polynomial. By Chien
             * Search
             */
                int temp = reg[0];
                COPY(ref reg, ref lambda, NN - KK);
                reg[0] = temp;
                count = 0;      /* Number of roots of lambda(x) */
                for(i = 1; i <= NN; i++)
                {
                    q = 1;
                    for(j = deg_lambda; j > 0; j--)
                        if(reg[j] != A0)
                        {
                            reg[j] = modnn(reg[j] + j);
                            q ^= Alpha_to[reg[j]];
                        }
                    if(q == 0)
                    {
                        /* store root (index-form) and error location number */
                        root[count] = i;
                        loc[count] = NN - i;
                        count++;
                    }
                }

#if DEBUG
                DicConsole.DebugWriteLine("Reed Solomon", "\n Final error positions:\t");
                for(i = 0; i < count; i++)
                    DicConsole.DebugWriteLine("Reed Solomon", "{0} ", loc[i]);
                DicConsole.DebugWriteLine("Reed Solomon", "\n");
#endif

                if(deg_lambda != count)
                {
                    /*
                 * deg(lambda) unequal to number of roots => uncorrectable
                 * error detected
                 */
                    return -1;
                }
                /*
             * Compute err+eras evaluator poly omega(x) = s(x)*lambda(x) (modulo
             * x**(NN-KK)). in index form. Also find deg(omega).
             */
                deg_omega = 0;
                for(i = 0; i < NN - KK; i++)
                {
                    tmp = 0;
                    j = (deg_lambda < i) ? deg_lambda : i;
                    for(; j >= 0; j--)
                    {
                        if((s[i + 1 - j] != A0) && (lambda[j] != A0))
                            tmp ^= Alpha_to[modnn(s[i + 1 - j] + lambda[j])];
                    }
                    if(tmp != 0)
                        deg_omega = i;
                    omega[i] = Index_of[tmp];
                }
                omega[NN - KK] = A0;

                /*
             * Compute error values in poly-form. num1 = omega(inv(X(l))), num2 =
             * inv(X(l))**(B0-1) and den = lambda_pr(inv(X(l))) all in poly-form
             */
                for(j = count - 1; j >= 0; j--)
                {
                    num1 = 0;
                    for(i = deg_omega; i >= 0; i--)
                    {
                        if(omega[i] != A0)
                            num1 ^= Alpha_to[modnn(omega[i] + i * root[j])];
                    }
                    num2 = Alpha_to[modnn(root[j] * (B0 - 1) + NN)];
                    den = 0;

                    /* lambda[i+1] for i even is the formal derivative lambda_pr of lambda[i] */
                    for(i = min(deg_lambda, NN - KK - 1) & ~1; i >= 0; i -= 2)
                    {
                        if(lambda[i + 1] != A0)
                            den ^= Alpha_to[modnn(lambda[i + 1] + i * root[j])];
                    }
                    if(den == 0)
                    {
                        DicConsole.DebugWriteLine("Reed Solomon", "\n ERROR: denominator = 0\n");
                        return -1;
                    }
                    /* Apply error to data */
                    if(num1 != 0)
                    {
                        data[loc[j]] ^= Alpha_to[modnn(Index_of[num1] + Index_of[num2] + NN - Index_of[den])];
                    }
                }
                return count;
            }
            throw new UnauthorizedAccessException("Trying to calculate RS without initializing!");
        }
    }
}
