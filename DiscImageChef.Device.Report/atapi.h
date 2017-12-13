//
// Created by claunia on 12/12/17.
//

#ifndef DISCIMAGECHEF_DEVICE_REPORT_ATAPI_H
#define DISCIMAGECHEF_DEVICE_REPORT_ATAPI_H
int IdentifyPacket(int fd, unsigned char **buffer, AtaErrorRegistersCHS **errorRegisters);
#endif //DISCIMAGECHEF_DEVICE_REPORT_ATAPI_H
