/* Public domain code from Info/Zip
 * Written by Kai Uew Rommel
 * from zip30/os2/os2zip.c
 * Modified by Natalia Portillo
 */

#include <stdlib.h>
#include <string.h>

#define INCL_NOPM
#define INCL_DOSNLS
#define INCL_DOSERRORS

#include <os2.h>

#include "ea.h"

void GetEAs(char *path, char **bufptr, size_t *size)
{
    FILESTATUS4 fs;
    PDENA2      pDENA, pFound;
    EAOP2       eaop;
    PGEA2       pGEA;
    PGEA2LIST   pGEAlist;
    PFEA2LIST   pFEAlist;
    ULONG       ulAttributes;
    ULONG       nLength;
    ULONG       nBlock;

    if(DosQueryPathInfo(path, FIL_QUERYEASIZE, (PBYTE) & fs, sizeof(fs)))
        return;

    nBlock = max(fs.cbList, 65535);

    if((pDENA = malloc((size_t)nBlock)) == NULL)
        return;

    ulAttributes = -1;

    if(DosEnumAttribute(ENUMEA_REFTYPE_PATH, path, 1, pDENA, nBlock, &ulAttributes, ENUMEA_LEVEL_NO_VALUE) ||
       ulAttributes == 0 || (pGEAlist = malloc((size_t)nBlock)) == NULL)
    {
        free(pDENA);
        return;
    }

    pGEA = pGEAlist->list;
    memset(pGEAlist, 0, nBlock);
    pFound = pDENA;

    while(ulAttributes--)
    {
        pGEA->cbName = pFound->cbName;
        strcpy(pGEA->szName, pFound->szName);

        nLength = sizeof(GEA2) + strlen(pGEA->szName);
        nLength = ((nLength - 1) / sizeof(ULONG) + 1) * sizeof(ULONG);

        pGEA->oNextEntryOffset = ulAttributes ? nLength : 0;
        pGEA = (PGEA2)((PCH)pGEA + nLength);

        pFound = (PDENA2)((PCH)pFound + pFound->oNextEntryOffset);
    }

    if(pGEA == pGEAlist->list) // No attributes to save
    {
        free(pDENA);
        free(pGEAlist);
        return;
    }

    pGEAlist->cbList = (PCH)pGEA - (PCH)pGEAlist;

    pFEAlist = (PVOID)pDENA; // reuse buffer
    pFEAlist->cbList = nBlock;

    eaop.fpGEA2List = pGEAlist;
    eaop.fpFEA2List = pFEAlist;
    eaop.oError     = 0;

    if(DosQueryPathInfo(path, FIL_QUERYEASFROMLIST, (PBYTE) & eaop, sizeof(eaop)))
    {
        free(pDENA);
        free(pGEAlist);
        return;
    }

    *size   = pFEAlist->cbList;
    *bufptr = (char *)pFEAlist;

    free(pDENA);
    free(pGEAlist);
}
