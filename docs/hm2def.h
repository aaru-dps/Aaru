/* <vms/lib/hm2def.h>
 *
 *	F11DEF - home block definitions for ODS level 2		[not in Starlet]
 */
#ifndef _HM2DEF_H
#define _HM2DEF_H

/*
   Home block definitions for Files-11 Structure Level 2
 */
#define HM2$C_LEVEL1	257	/* 401 octal = structure level 1 */
#define HM2$C_LEVEL2	512	/* 1000 octal = structure level 2 */
#define HM2$V_READCHECK		0
#define HM2$V_WRITCHECK		1
#define HM2$V_ERASE		2
#define HM2$V_NOHIGHWATER	3
#define HM2$V_CLASS_PROT	4
#define HM2$M_READCHECK		(1<<HM2$V_READCHECK)	/* 0x01 */
#define HM2$M_WRITCHECK		(1<<HM2$V_WRITCHECK)	/* 0x02 */
#define HM2$M_ERASE		(1<<HM2$V_ERASE)	/* 0x04 */
#define HM2$M_NOHIGHWATER	(1<<HM2$V_NOHIGHWATER)	/* 0x08 */
#define HM2$M_CLASS_PROT	(1<<HM2$V_CLASS_PROT)	/* 0x10 */

#define HM2$S_CREDATE	8
#define HM2$S_RETAINMIN 8
#define HM2$S_RETAINMAX 8
#define HM2$S_REVDATE	8
#define HM2$S_MIN_CLASS 20
#define HM2$S_MAX_CLASS 20
#define HM2$S_FILETAB_FID 6
#define HM2$S_STRUCNAME 12
#define HM2$S_VOLNAME	12
#define HM2$S_OWNERNAME 12
#define HM2$S_FORMAT	12

#define HM2$S_HM2DEF	512
struct hm2def {
    unsigned long hm2$l_homelbn;	/* LBN of home (i.e., this) block */
    unsigned long hm2$l_alhomelbn;	/* LBN of alternate home block */
    unsigned long hm2$l_altidxlbn;	/* LBN of alternate index file header */
    union {
	unsigned short hm2$w_struclev;		/* volume structure level */
	struct {
	    unsigned char hm2$b_strucver;	/* structure version number */
	    unsigned char hm2$b_struclev;	/* main structure level */
	} hm2$r_structlev_fields;
    } hm2$r_structlev_overlay;
    unsigned short hm2$w_cluster;	/* storage bitmap cluster factor */
    unsigned short hm2$w_homevbn;	/* VBN of home (i.e., this) block */
    unsigned short hm2$w_alhomevbn;	/* VBN of alternate home block */
    unsigned short hm2$w_altidxvbn;	/* VBN of alternate index file header */
    unsigned short hm2$w_ibmapvbn;	/* VBN of index file bitmap */
    unsigned long hm2$l_ibmaplbn;	/* LBN of index file bitmap */
    unsigned long hm2$l_maxfiles;	/* maximum ! files on volume */
    unsigned short hm2$w_ibmapsize;	/* index file bitmap size, blocks */
    unsigned short hm2$w_resfiles;	/* ! reserved files on volume */
    unsigned short hm2$w_devtype;	/* disk device type */
    unsigned short hm2$w_rvn;		/* relative volume number of this volume */
    unsigned short hm2$w_setcount;	/* count of volumes in set */
    union {
	unsigned short hm2$w_volchar;	/* volume characteristics */
	struct {
	    unsigned hm2$v_readcheck	: 1; /* verify all read operations */
	    unsigned hm2$v_writcheck	: 1; /* verify all write operations */
	    unsigned hm2$v_erase	: 1; /* erase all files on delete */
	    unsigned hm2$v_nohighwater	: 1; /* turn off high-water marking */
	    unsigned hm2$v_class_prot	: 1; /* enable classification checks on the volume */
	    unsigned			: 3; /* padding */
	} hm2$r_volchar_bits;
    } hm2$r_volchar_overlay;
    unsigned long hm2$l_volowner;	/* volume owner UIC */
    unsigned long hm2$l_sec_mask;	/* volume security mask */
    unsigned short hm2$w_protect;	/* volume protection */
    unsigned short hm2$w_fileprot;	/* default file protection */
    unsigned short hm2$w_recprot;	/* default file record protection */
    unsigned short hm2$w_checksum1;	/* first checksum */
    long hm2$q_credate[2];		/* volume creation date */
    unsigned char hm2$b_window;		/* default window size */
    unsigned char hm2$b_lru_lim;	/* default LRU limit */
    unsigned short hm2$w_extend;	/* default file extend */
    long hm2$q_retainmin[2];		/* minimum file retention period */
    long hm2$q_retainmax[2];		/* maximum file retention period */
    long hm2$q_revdate[2];		/* volume revision date */
    struct {
	char hm2$r_min_class[20];	/* volume minimum security class */
    } hm2$r_min_class;
    struct {
	char hm2$r_max_class[20];	/* volume maximum security class */
    } hm2$r_max_class;
    unsigned short hm2$w_filetab_fid[3]; /* file lookup table FID */
    union {
	unsigned short hm2$w_lowstruclev;	/* lowest struclev on volume */
	struct {
	    unsigned char hm2$b_lowstrucver;	/* structure version number */
	    unsigned char hm2$b_lowstruclev;	/* main structure level */
	} hm2$r_lowstruclev_fields;
    } hm2$r_lowstruclev_overlay;
    union {
	unsigned short hm2$w_highstruclev;	/* highest struclev on volume */
	struct {
	    unsigned char hm2$b_highstrucver;	/* structure version number */
	    unsigned char hm2$b_highstruclev;	/* main structure level */
	} hm2$r_highstruclev_fields;
    } hm2$r_highstruclev_overlay;
    long hm2$q_copydate[2];		/* volume copy date (V6) */
    char hm2def$$_fill_1[302];		/* spare */
    unsigned long hm2$l_serialnum;	/* pack serial number */
    char hm2$t_strucname[12];		/* structure (volume set name) */
    char hm2$t_volname[12];		/* volume name */
    char hm2$t_ownername[12];		/* volume owner name */
    char hm2$t_format[12];		/* volume format type */
    unsigned	: 8, : 8;		/* char fill[2]; spare */
    unsigned short hm2$w_checksum2;	/* second checksum */
};

/* Type of homeblock placement deltas. */
#define HM2$C_REQ_DELTA_GEOM_DEPEND	0	/* dependent on disk geometry */
#define HM2$C_REQ_DELTA_GEOM_INDEPEND	1	/* independent of disk geometry */
#define HM2$C_REQ_DELTA_FIXED_CONTIG	2	/* fixed so index file will be contig (for Dollar) */
#define HM2$C_GEOM_INDEPEND_DELTA	1033	/* actual geometry independent delta -*/
						/*+ this is a prime > 1000 */
#define HM2$C_FIXED_CONTIG_DELTA	1	/* fixed delta for contiguous index file */
#define HM2$C_LIMITED_SEARCH_LENGTH	10	/* number of blocks to check in a limited search */

#endif	/*_HM2DEF_H*/
