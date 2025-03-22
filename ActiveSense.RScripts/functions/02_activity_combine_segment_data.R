
#' @name combine_segment_data
#' @title combining segmented data
#'
#' @description Classify 24 hours of data and combining them to the previous section
#'
#' @param binfile A filename of a GENEActiv.bin to process.
#' @param start_time Start_time is split the days up by
#' @param datacols see segmentation in GENEAclassify
#' @param mmap.load see read.bin in GENEAread
#'

combine_segment_data = function(binfile,
                                 start_time,
                                 datacols,
                                 mmap.load = T) {

  # Naming of the csv file protocol.
  dataname = naming_protocol(binfile)

  # Take the header file
  header = header.info(binfile)

  # Check to see if the CSV file for this exists already
  if (!file.exists(file.path(getwd(), "/outputs/", dataname))) {

    # Finding the first time
    AccData1 = read.bin(binfile, start = 0, end = 0.01)
    First_Time = AccData1$data.out[1, 1]
    
    # Need to add the first_time at 03:00! Dam 

    # Now I need to check that there is at least 24 hours of data
    # Last Time
    AccData2 = read.bin(binfile, start = 0.99, end = 1)
    Last_Time = AccData2$data.out[length(AccData2$data.out[, 1]), 1]

    # Should return this as the first and last time
    DayNo = as.numeric(ceiling((Last_Time - First_Time) / 86400))
    
    First_Start_Time  = as.POSIXct(First_Time, origin = "1970-01-01", tz = "GMT")
    First_Start_Time = as.Date(First_Start_Time)
    First_Start_time = as.POSIXct(paste(First_Start_Time, start_time), origin = "1970-01-01", tz = "GMT")
    First_Start_time = as.numeric(First_Start_time)
    
    # Is the start time before or after the first record time? 
    
    if (First_Time > First_Start_time){
      First_Start_time = First_Start_time + 86400
      DayNo = DayNo - 1
    } 

    # Initialize the segmented data
    segment_data = c()
    
    for (i in 0:DayNo) {
      if (i == 0){
        segment_data1 = getGENEAsegments(binfile,
                                          start = First_Time,
                                          end = First_Start_time,
                                          keep_raw_data = TRUE,
                                          mmap.load = mmap.load,
                                          Use.Timestamps = TRUE,
                                          changepoint = "UpDownMeanVarDegreesMeanVar",
                                          penalty = "Manual",
                                          pen.value1 = 40,
                                          pen.value2 = 400,
                                          datacols = datacols,
                                          intervalseconds = 30,
                                          mininterval = 1,
                                          downsample = as.numeric(unlist(header$Value[2]),
                                          samplefreq = as.numeric(unlist(header$Value[2])), 
                                          filterorder = 2, 
                                          boundaries = c(0.5, 5),
                                          Rp = 3,
                                          hysteresis = 0.1)
        )
      } else if (i == DayNo){
        segment_data1 = getGENEAsegments(binfile,
                                          start = First_Start_time + 86400 * (i - 1) ,
                                          end = Last_Time,
                                          keep_raw_data = TRUE,
                                          mmap.load = mmap.load,
                                          Use.Timestamps = TRUE,
                                          changepoint = "UpDownMeanVarDegreesMeanVar",
                                          penalty = "Manual",
                                          pen.value1 = 40,
                                          pen.value2 = 400,
                                          datacols = datacols,
                                          intervalseconds = 30,
                                          mininterval = 1,
                                          downsample = as.numeric(unlist(header$Value[2])),
                                          samplefreq = as.numeric(unlist(header$Value[2])), 
                                          filterorder = 2, 
                                          boundaries = c(0.5, 5),
                                          Rp = 3,
                                          hysteresis = 0.1)
      } else {
        segment_data1 = getGENEAsegments(binfile,
                                          start = First_Start_time + 86400 * (i - 1),
                                          end = First_Start_time + 86400 * i,
                                          keep_raw_data = TRUE,
                                          mmap.load = mmap.load,
                                          Use.Timestamps = TRUE,
                                          changepoint = "UpDownMeanVarDegreesMeanVar",
                                          penalty = "Manual",
                                          pen.value1 = 40,
                                          pen.value2 = 400,
                                          datacols = datacols,
                                          intervalseconds = 30,
                                          mininterval = 1,
                                          downsample = as.numeric(unlist(header$Value[2])),
                                          samplefreq = as.numeric(unlist(header$Value[2])), 
                                          filterorder = 2, 
                                          boundaries = c(0.5, 5),
                                          Rp = 3,
                                          hysteresis = 0.1)
        
      }
      
    segment_data = rbind(segment_data, segment_data1)
    }
  } else {
    segment_data = readRDS(file.path(getwd(), "/outputs/", dataname))
  }
  return(segment_data)
}
