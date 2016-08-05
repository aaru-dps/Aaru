#define	S_S3MAGIC	0x2b5544	/* system 3 arbitrary magic value */
#define	S_CLEAN	0106        	/* arbitrary magic value  */

/* s_type, block size of file system */
#define	S_B512		1	/* 512 byte block */
#define	S_B1024		2	/* 1024 byte block */

/* codes for file system version (for utilities) */
#define	S_V2		1		/* version 7 */
#define	S_V3		2		/* system 3 */

				/* Note: DIRSIZ must be even, or else add
				 *       filler byte to direct struct.
				 */
#define	DIRSIZ	14

#define NICINOD 100              /* number of superblock inodes */

#ifdef LISA

#define NICFREE 50               /* number of superblock free blocks */
#define NSBFILL 51              /* aligns s_magic & s_type at end of SB blk */

#else

#define NICFREE 100              /* number of superblock free blocks */

#ifdef XENIX_2.2.1e

#define    NSBFILL 370             /* aligns s_magic & s_type at end of SB blk */

#else

#define    NSBFILL 371             /* aligns s_magic, .. at end of super block */

#endif

#endif


typedef long             daddr_t;        /* disc address */
typedef unsigned short   ino_t;          /* i-node number */
typedef long             time_t;         /* time (seconds) */
typedef long             off_t;          /* offset in file */

struct	filsys
{
	ushort	s_isize;	/* size in blocks of i-list */
	daddr_t	s_fsize;	/* size in blocks of entire volume */
	short	s_nfree;	/* number of addresses in s_free */
	daddr_t	s_free[NICFREE];	/* free block list */
	short	s_ninode;	/* number of i-nodes in s_inode */
	ino_t	s_inode[NICINOD];	/* free i-node list */
	char	s_flock;	/* lock during free list manipulation */
	char	s_ilock;	/* lock during i-list manipulation */
	char  	s_fmod; 	/* super block modified flag */
	char	s_ronly;	/* mounted read-only flag */
	time_t	s_time; 	/* last super block update */
	daddr_t	s_tfree;	/* total free blocks*/
	ino_t	s_tinode;	/* total free inodes */
	short   s_dinfo[4];     /* device information */
	char	s_fname[6];	/* file system name */
	char	s_fpack[6];	/* file system pack name */
	/* remainder is maintained for xenix */
	char   	s_clean;   	/* S_CLEAN if structure is properly closed */
	char    s_fill[NSBFILL];/* space to make sizeof filsys be BSIZE */
	long    s_magic;        /* indicates version of filsys */
	long	s_type;		/* type of new file system */
};

struct	fblk
{
	short	df_nfree;
	daddr_t	df_free[NICFREE];
};

struct	direct
{
	ino_t	d_ino;
	char	d_name[DIRSIZ];
};

	/* Inode structure as it appears on a disk block. */
struct dinode
{
	ushort di_mode;		/* mode and type of file */
	short	di_nlink;    	/* number of links to file */
	ushort	di_uid;      	/* owner's user id */
	ushort	di_gid;      	/* owner's group id */
	off_t	di_size;     	/* number of bytes in file */
	char  	di_addr[40];	/* disk block addresses */
	time_t	di_atime;   	/* time last accessed */
	time_t	di_mtime;   	/* time last modified */
	time_t	di_ctime;   	/* time created */
};
/*
 * the 40 address bytes:
 *	39 used; 13 addresses
 *	of 3 bytes each.
 */
