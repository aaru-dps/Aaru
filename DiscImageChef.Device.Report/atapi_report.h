//
// Created by claunia on 13/12/17.
//

#ifndef DISCIMAGECHEF_DEVICE_REPORT_ATAPI_REPORT_H
#define DISCIMAGECHEF_DEVICE_REPORT_ATAPI_REPORT_H

#include <libxml/xmlwriter.h>
#define DIC_ATAPI_REPORT_ELEMENT "ATAPI"

void AtapiReport(int fd, xmlTextWriterPtr xmlWriter);
#endif //DISCIMAGECHEF_DEVICE_REPORT_ATAPI_REPORT_H
