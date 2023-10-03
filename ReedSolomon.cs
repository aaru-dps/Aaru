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
// Copyright © 2011-2023 Natalia Portillo
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

namespace Aaru.Checksums;

/// <summary>Implements the Reed-Solomon algorithm</summary>
public class ReedSolomon
{
    /// <summary>Alpha exponent for the first root of the generator polynomial</summary>
    const int B0 = 1;
    const string MODULE_NAME = "Reed Solomon";
    /// <summary>No legal value in index form represents zero, so we need a special value for this purpose</summary>
    int _a0;
    /// <summary>index->polynomial form conversion table</summary>
    int[] _alphaTo;
    /// <summary>Generator polynomial g(x) Degree of g(x) = 2*TT has roots @**B0, @**(B0+1), ... ,@^(B0+2*TT-1)</summary>
    int[] _gg;
    /// <summary>Polynomial->index form conversion table</summary>
    int[] _indexOf;
    bool _initialized;
    int  _mm, _kk, _nn;
    /// <summary>
    ///     Primitive polynomials - see Lin & Costello, Error Control Coding Appendix A, and Lee & Messerschmitt, Digital
    ///     Communication p. 453.
    /// </summary>
    int[] _pp;

    /// <summary>Initializes the Reed-Solomon with RS(n,k) with GF(2^m)</summary>
    public void InitRs(int n, int k, int m)
    {
        _pp = m switch
              {
                  2  => new[] { 1, 1, 1 },
                  3  => new[] { 1, 1, 0, 1 },
                  4  => new[] { 1, 1, 0, 0, 1 },
                  5  => new[] { 1, 0, 1, 0, 0, 1 },
                  6  => new[] { 1, 1, 0, 0, 0, 0, 1 },
                  7  => new[] { 1, 0, 0, 1, 0, 0, 0, 1 },
                  8  => new[] { 1, 0, 1, 1, 1, 0, 0, 0, 1 },
                  9  => new[] { 1, 0, 0, 0, 1, 0, 0, 0, 0, 1 },
                  10 => new[] { 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1 },
                  11 => new[] { 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                  12 => new[] { 1, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1 },
                  13 => new[] { 1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                  14 => new[] { 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 },
                  15 => new[] { 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                  16 => new[] { 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1 },
                  _ => throw new ArgumentOutOfRangeException(
                           nameof(m), Localization.m_must_be_between_2_and_16_inclusive)
              };

        _mm      = m;
        _kk      = k;
        _nn      = n;
        _a0      = n;
        _alphaTo = new int[n + 1];
        _indexOf = new int[n + 1];

        _gg = new int[_nn - _kk + 1];

        generate_gf();
        gen_poly();

        _initialized = true;
    }

    int Modnn(int x)
    {
        while(x >= _nn)
        {
            x -= _nn;
            x =  (x >> _mm) + (x & _nn);
        }

        return x;
    }

    static int Min(int a, int b) => a < b ? a : b;

    static void Clear(ref int[] a, int n)
    {
        int ci;

        for(ci = n - 1; ci >= 0; ci--)
            a[ci] = 0;
    }

    static void Copy(ref int[] a, ref int[] b, int n)
    {
        int ci;

        for(ci = n - 1; ci >= 0; ci--)
            a[ci] = b[ci];
    }

    static void Copydown(ref int[] a, ref int[] b, int n)
    {
        int ci;

        for(ci = n - 1; ci >= 0; ci--)
            a[ci] = b[ci];
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

        var mask = 1;
        _alphaTo[_mm] = 0;

        for(i = 0; i < _mm; i++)
        {
            _alphaTo[i]           = mask;
            _indexOf[_alphaTo[i]] = i;

            /* If Pp[i] == 1 then, term @^i occurs in poly-repr of @^MM */
            if(_pp[i] != 0)
                _alphaTo[_mm] ^= mask; /* Bit-wise EXOR operation */

            mask <<= 1; /* single left-shift */
        }

        _indexOf[_alphaTo[_mm]] = _mm;
        /*
         * Have obtained poly-repr of @^MM. Poly-repr of @^(i+1) is given by
         * poly-repr of @^i shifted left one-bit and accounting for any @^MM
         * term that may occur when poly-repr of @^i is shifted.
         */
        mask >>= 1;

        for(i = _mm + 1; i < _nn; i++)
        {
            if(_alphaTo[i - 1] >= mask)
                _alphaTo[i] = _alphaTo[_mm] ^ (_alphaTo[i - 1] ^ mask) << 1;
            else
                _alphaTo[i] = _alphaTo[i - 1] << 1;

            _indexOf[_alphaTo[i]] = i;
        }

        _indexOf[0]   = _a0;
        _alphaTo[_nn] = 0;
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

        _gg[0] = _alphaTo[B0];
        _gg[1] = 1; /* g(x) = (X+@**B0) initially */

        for(i = 2; i <= _nn - _kk; i++)
        {
            _gg[i] = 1;

            /*
             * Below multiply (Gg[0]+Gg[1]*x + ... +Gg[i]x^i) by
             * (@**(B0+i-1) + x)
             */
            for(int j = i - 1; j > 0; j--)
            {
                if(_gg[j] != 0)
                    _gg[j] = _gg[j - 1] ^ _alphaTo[Modnn(_indexOf[_gg[j]] + B0 + i - 1)];
                else
                    _gg[j] = _gg[j - 1];
            }

            /* Gg[0] can never be zero */
            _gg[0] = _alphaTo[Modnn(_indexOf[_gg[0]] + B0 + i - 1)];
        }

        /* convert Gg[] to index form for quicker encoding */
        for(i = 0; i <= _nn - _kk; i++)
            _gg[i] = _indexOf[_gg[i]];
    }

    /*
     * take the string of symbols in data[i], i=0..(k-1) and encode
     * systematically to produce NN-KK parity symbols in bb[0]..bb[NN-KK-1] data[]
     * is input and bb[] is output in polynomial form. Encoding is done by using
     * a feedback shift register with appropriate connections specified by the
     * elements of Gg[], which was generated above. Codeword is   c(X) =
     * data(X)*X**(NN-KK)+ b(X)
     */
    /// <summary>Takes the symbols in data to output parity in bb.</summary>
    /// <returns>Returns -1 if an illegal symbol is found.</returns>
    /// <param name="data">Data symbols.</param>
    /// <param name="bb">Outs parity symbols.</param>
    public int encode_rs(int[] data, out int[] bb)
    {
        if(!_initialized)
            throw new UnauthorizedAccessException(Localization.Trying_to_calculate_RS_without_initializing);

        int i;
        bb = new int[_nn - _kk];

        Clear(ref bb, _nn - _kk);

        for(i = _kk - 1; i >= 0; i--)
        {
            if(_mm != 8)
            {
                if(data[i] > _nn)
                    return -1; /* Illegal symbol */
            }

            int feedback = _indexOf[data[i] ^ bb[_nn - _kk - 1]];

            if(feedback != _a0)
            {
                /* feedback term is non-zero */
                for(int j = _nn - _kk - 1; j > 0; j--)
                {
                    if(_gg[j] != _a0)
                        bb[j] = bb[j - 1] ^ _alphaTo[Modnn(_gg[j] + feedback)];
                    else
                        bb[j] = bb[j - 1];
                }

                bb[0] = _alphaTo[Modnn(_gg[0] + feedback)];
            }
            else
            {
                /* feedback term is zero. encoder becomes a
                 * single-byte shifter */
                for(int j = _nn - _kk - 1; j > 0; j--)
                    bb[j] = bb[j - 1];

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
    /// <summary>Decodes the RS. If decoding is successful outputs corrected data symbols.</summary>
    /// <returns>Returns corrected symbols, -1 if illegal or uncorrectable</returns>
    /// <param name="data">Data symbols.</param>
    /// <param name="erasPos">Position of erasures.</param>
    /// <param name="noEras">Number of erasures.</param>
    public int eras_dec_rs(ref int[] data, out int[] erasPos, int noEras)
    {
        if(!_initialized)
            throw new UnauthorizedAccessException(Localization.Trying_to_calculate_RS_without_initializing);

        erasPos = new int[_nn - _kk];
        int i, j;
        int q, tmp;
        var recd   = new int[_nn];
        var lambda = new int[_nn - _kk + 1]; /* Err+Eras Locator poly */
        var s      = new int[_nn - _kk + 1]; /* syndrome poly */
        var b      = new int[_nn - _kk + 1];
        var t      = new int[_nn - _kk + 1];
        var omega  = new int[_nn - _kk + 1];
        var root   = new int[_nn       - _kk];
        var reg    = new int[_nn - _kk + 1];
        var loc    = new int[_nn       - _kk];
        int count;

        /* data[] is in polynomial form, copy and convert to index form */
        for(i = _nn - 1; i >= 0; i--)
        {
            if(_mm != 8)
            {
                if(data[i] > _nn)
                    return -1; /* Illegal symbol */
            }

            recd[i] = _indexOf[data[i]];
        }

        /* first form the syndromes; i.e., evaluate recd(x) at roots of g(x)
         * namely @**(B0+i), i = 0, ... ,(NN-KK-1)
         */
        var synError = 0;

        for(i = 1; i <= _nn - _kk; i++)
        {
            tmp = 0;

            for(j = 0; j < _nn; j++)
            {
                if(recd[j] != _a0) /* recd[j] in index form */
                    tmp ^= _alphaTo[Modnn(recd[j] + (B0 + i - 1) * j)];
            }

            synError |= tmp; /* set flag if non-zero syndrome =>
                              * error */

            /* store syndrome in index form  */
            s[i] = _indexOf[tmp];
        }

        if(synError == 0)
            return 0;

        Clear(ref lambda, _nn - _kk);
        lambda[0] = 1;

        if(noEras > 0)
        {
            /* Init lambda to be the erasure locator polynomial */
            lambda[1] = _alphaTo[erasPos[0]];

            for(i = 1; i < noEras; i++)
            {
                int u = erasPos[i];

                for(j = i + 1; j > 0; j--)
                {
                    tmp = _indexOf[lambda[j - 1]];

                    if(tmp != _a0)
                        lambda[j] ^= _alphaTo[Modnn(u + tmp)];
                }
            }

        #if DEBUG
            /* find roots of the erasure location polynomial */
            for(i = 1; i <= noEras; i++)
                reg[i] = _indexOf[lambda[i]];

            count = 0;

            for(i = 1; i <= _nn; i++)
            {
                q = 1;

                for(j = 1; j <= noEras; j++)
                {
                    if(reg[j] != _a0)
                    {
                        reg[j] =  Modnn(reg[j] + j);
                        q      ^= _alphaTo[reg[j]];
                    }
                }

                if(q != 0)
                    continue;

                /* store root and error location
                 * number indices
                 */
                root[count] = i;
                loc[count]  = _nn - i;
                count++;
            }

            if(count != noEras)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.lambda_is_wrong);

                return -1;
            }

            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       Localization.Erasure_positions_as_determined_by_roots_of_Eras_Loc_Poly);

            for(i = 0; i < count; i++)
                AaruConsole.DebugWriteLine(MODULE_NAME, "{0} ", loc[i]);

            AaruConsole.DebugWriteLine(MODULE_NAME, "\n");
        #endif
        }

        for(i = 0; i < _nn - _kk + 1; i++)
            b[i] = _indexOf[lambda[i]];

        /*
         * Begin Berlekamp-Massey algorithm to determine error+erasure
         * locator polynomial
         */
        int r  = noEras;
        int el = noEras;

        while(++r <= _nn - _kk)
        {
            /* r is the step number */
            /* Compute discrepancy at the r-th step in poly-form */
            var discrR = 0;

            for(i = 0; i < r; i++)
            {
                if(lambda[i] != 0 &&
                   s[r - i]  != _a0)
                    discrR ^= _alphaTo[Modnn(_indexOf[lambda[i]] + s[r - i])];
            }

            discrR = _indexOf[discrR]; /* Index form */

            if(discrR == _a0)
            {
                /* 2 lines below: B(x) <-- x*B(x) */
                Copydown(ref b, ref b, _nn - _kk);
                b[0] = _a0;
            }
            else
            {
                /* 7 lines below: T(x) <-- lambda(x) - discr_r*x*b(x) */
                t[0] = lambda[0];

                for(i = 0; i < _nn - _kk; i++)
                {
                    if(b[i] != _a0)
                        t[i + 1] = lambda[i + 1] ^ _alphaTo[Modnn(discrR + b[i])];
                    else
                        t[i + 1] = lambda[i + 1];
                }

                if(2 * el <= r + noEras - 1)
                {
                    el = r + noEras - el;

                    /*
                     * 2 lines below: B(x) <-- inv(discr_r) *
                     * lambda(x)
                     */
                    for(i = 0; i <= _nn - _kk; i++)
                        b[i] = lambda[i] == 0 ? _a0 : Modnn(_indexOf[lambda[i]] - discrR + _nn);
                }
                else
                {
                    /* 2 lines below: B(x) <-- x*B(x) */
                    Copydown(ref b, ref b, _nn - _kk);
                    b[0] = _a0;
                }

                Copy(ref lambda, ref t, _nn - _kk + 1);
            }
        }

        /* Convert lambda to index form and compute deg(lambda(x)) */
        var degLambda = 0;

        for(i = 0; i < _nn - _kk + 1; i++)
        {
            lambda[i] = _indexOf[lambda[i]];

            if(lambda[i] != _a0)
                degLambda = i;
        }

        /*
         * Find roots of the error+erasure locator polynomial. By Chien
         * Search
         */
        int temp = reg[0];
        Copy(ref reg, ref lambda, _nn - _kk);
        reg[0] = temp;
        count  = 0; /* Number of roots of lambda(x) */

        for(i = 1; i <= _nn; i++)
        {
            q = 1;

            for(j = degLambda; j > 0; j--)
            {
                if(reg[j] != _a0)
                {
                    reg[j] =  Modnn(reg[j] + j);
                    q      ^= _alphaTo[reg[j]];
                }
            }

            if(q != 0)
                continue;

            /* store root (index-form) and error location number */
            root[count] = i;
            loc[count]  = _nn - i;
            count++;
        }

    #if DEBUG
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Final_error_positions);

        for(i = 0; i < count; i++)
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0} ", loc[i]);

        AaruConsole.DebugWriteLine(MODULE_NAME, "\n");
    #endif

        if(degLambda != count)
            return -1;

        /*
         * Compute err+eras evaluator poly omega(x) = s(x)*lambda(x) (modulo
         * x**(NN-KK)). in index form. Also find deg(omega).
         */
        var degOmega = 0;

        for(i = 0; i < _nn - _kk; i++)
        {
            tmp = 0;
            j   = degLambda < i ? degLambda : i;

            for(; j >= 0; j--)
            {
                if(s[i + 1 - j] != _a0 &&
                   lambda[j]    != _a0)
                    tmp ^= _alphaTo[Modnn(s[i + 1 - j] + lambda[j])];
            }

            if(tmp != 0)
                degOmega = i;

            omega[i] = _indexOf[tmp];
        }

        omega[_nn - _kk] = _a0;

        /*
         * Compute error values in poly-form. num1 = omega(inv(X(l))), num2 =
         * inv(X(l))**(B0-1) and den = lambda_pr(inv(X(l))) all in poly-form
         */
        for(j = count - 1; j >= 0; j--)
        {
            var num1 = 0;

            for(i = degOmega; i >= 0; i--)
            {
                if(omega[i] != _a0)
                    num1 ^= _alphaTo[Modnn(omega[i] + i * root[j])];
            }

            int num2 = _alphaTo[Modnn(root[j] * (B0 - 1) + _nn)];
            var den  = 0;

            /* lambda[i+1] for i even is the formal derivative lambda_pr of lambda[i] */
            for(i = Min(degLambda, _nn - _kk - 1) & ~1; i >= 0; i -= 2)
            {
                if(lambda[i + 1] != _a0)
                    den ^= _alphaTo[Modnn(lambda[i + 1] + i * root[j])];
            }

            if(den == 0)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.ERROR_denominator_equals_zero);

                return -1;
            }

            /* Apply error to data */
            if(num1 != 0)
                data[loc[j]] ^= _alphaTo[Modnn(_indexOf[num1] + _indexOf[num2] + _nn - _indexOf[den])];
        }

        return count;
    }
}