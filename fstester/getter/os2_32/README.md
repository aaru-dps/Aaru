DiscImageChef 32-bit OS/2 Filesystem Getter
===========================================
This snippet is designed to retrieve all Extended Attributes from all files in the `C:`
volume, saving them as a set of COUNTER.EA files in executing directory.

The format of those files should be as `FEA2LIST` structure (following).
It compiles under OpenWatcom.

```
/* FEA2 defines the format for setting the full extended attributes in the file. */

 typedef struct _FEA2 { 
   ULONG      oNextEntryOffset;  /*  Offset to next entry. */ 
   BYTE       fEA;               /*  Extended attributes flag. */ 
   BYTE       cbName;            /*  Length of szName, not including NULL. */ 
   USHORT     cbValue;           /*  Value length. */ 
   CHAR       szName[1];         /*  Extended attribute name. */ 
 } FEA2; 
 typedef FEA2 *PFEA2;

/* FEA2 data structure. */

 typedef struct _FEA2LIST { 
   ULONG     cbList;   /*  Total bytes of structure including full list. */ 
   FEA2      list[1];  /*  Variable-length FEA2 structures. */ 
 } FEA2LIST; 
  typedef FEA2LIST *PFEA2LIST;
```