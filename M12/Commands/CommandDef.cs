namespace M12.Commands
{
    /// <summary>
    /// The definition of commands supported by the bootloader.
    /// </summary>
    public enum CommandDef
    {
        HOST_CMD_CHECKMODE,                         // Check whether the host is in the appliction mode or the DFU mode.
        HOST_CMD_HOME,
        HOST_CMD_MOVE,
        HOST_CMD_MOVE_T_OUT,
        HOST_CMD_MOVE_T_ADC,
        HOST_CMD_FAST_MOVE,
        HOST_CMD_STOP,
        HOST_CMD_SET_ACC,
        HOST_CMD_SET_MODE,
        HOST_CMD_GET_SYS_INFO,
        HOST_CMD_GET_MCSU_STA,
        HOST_CMD_GET_MCSU_SETTINGS,
        HOST_CMD_GET_SYS_STA,
        HOST_CMD_GET_ERR,
        HOST_CMD_GET_MEM_LEN,
        HOST_CMD_READ_MEM,                            // read the content of the memory.
        HOST_CMD_CLEAR_MEM,                           // clear the memory/
        HOST_CMD_SET_DOUT,                            // set the digital output status.
        HOST_CMD_READ_DOUT,                           // read the status of the digital output.
        HOST_CMD_READ_DIN,                            // read the status of the digital input.
        HOST_CMD_READ_AD,                             // start a conversion and read the ADC value.
        HOST_CMD_SET_ADC_OSR,                         // set the OSR of the AD7606.
        HOST_CMD_EN_CSS,
        HOST_CMD_SET_CSSTHD,
        HOST_CMD_SET_T_ADC,
        HOST_CMD_SET_T_OUT,

        HOST_CMD_LINK_DIN_DOUT = 0xB0,

        HOST_CMD_BLINDSEARCH = 0xC0,
        HOST_CMD_SNAKESEARCH,

        HOST_CMD_SAV_MCSU_ENV = 0xD0,


        HOST_CMD_SYS_WR_DFUKEY = 0xE0,                // write the key to the SRAM to ask the MCU to enter the DFU mode after reset.
        HOST_CMD_SYS_RESET = 0xE1                     // reset the system.

    }

}
