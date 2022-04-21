from eudplib import *
#0x58DBEC = 접근 조건
#UsedMemory 58D740 + 20 * 60 = 58DBF0
#0x58DBF0 = 트리거 명령코드

#0x58DBF4 = 트리거 X
#0x58DBF8 = 트리거 Y
#0x58DBFC = 유닛 종류
#0x58DC00 = 플레이어
def onPluginStart():
    bytebuffer = bytearray([67,58,92,85,115,101,114,115,92,107,92,68,111,99,117,109,101,110,116,115,92,83,116,97,114,67,114,97,102,116,92,77,97,112,115,92,95,65,99,116,105,118,101,95,68,101,118,101,108,111,112,109,101,110,116,92,84,101,109,112,92,80,114,111,106,101,99,116,105,108,101,115,92,112,114,111,106,46,115,99,120])
    btnptr = Db(bytebuffer)
    DoActions([
        SetMemory(0x58DBEC, SetTo, btnptr),
        SetMemory(0x58DBF0, SetTo, 0),
        SetMemory(0x58DBF4, SetTo, 0),
        SetMemory(0x58DBF8, SetTo, 0),
        SetMemory(0x58DBFC, SetTo, 0),
        SetMemory(0x58DC00, SetTo, 0),
    ])





def beforeTriggerExec():
    if EUDIf()(Memory(0x58DBF0, Exactly, 1)): #액션 시작
        LocationPosX = f_dwread_epd(EPD(0x58DBF4))
        LocationPosY = f_dwread_epd(EPD(0x58DBF8))
        Unit = f_dwread_epd(EPD(0x58DBFC))
        Player = f_dwread_epd(EPD(0x58DC00))

        LocationOldPosX = EUDVariable()
        LocationOldPosY = EUDVariable()
        StarEditFlag = EUDVariable()

        LocationOldPosX << f_dwread_epd(EPD(0x58E154))
        LocationOldPosY << f_dwread_epd(EPD(0x58E158))
        StarEditFlag << f_dwread_epd(EPD(0x661518 + Unit * 2))

        DoActions([
        #58DC60 + 63 * 20
            SetMemory(0x661518 + Unit * 2, SetTo, 131074),
            SetMemory(0x58E14C, SetTo, LocationPosX),
            SetMemory(0x58E150, SetTo, LocationPosY),
            SetMemory(0x58E154, SetTo, LocationPosX),
            SetMemory(0x58E158, SetTo, LocationPosY),

            CreateUnit(1, Unit, 'Anywhere', Player),

            SetMemory(0x58E14C, SetTo, 0),
            SetMemory(0x58E150, SetTo, 0),
            SetMemory(0x58E154, SetTo, LocationOldPosX),
            SetMemory(0x58E158, SetTo, LocationOldPosY),


            SetMemory(0x661518 + Unit * 2, SetTo, StarEditFlag),


            SetMemory(0x58DBF0, SetTo, 0),
            SetMemory(0x58DBF4, SetTo, 0),
            SetMemory(0x58DBF8, SetTo, 0),
            SetMemory(0x58DBFC, SetTo, 0),
            SetMemory(0x58DC00, SetTo, 0),
        ])
    EUDEndIf()
