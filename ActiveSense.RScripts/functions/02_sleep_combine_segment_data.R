
#' @name combine_segment_data
#' @title combining segmented data
#'
#' @description Classify 24 hours of data and combining them to the previous section
#'
#' @param binfile A filename of a GENEActiv.bin to process.
#' @param start_time Start_time is split the days up by
#' @param datacols see segmentation in GENEAclassify
#' @param mmap.load see read.bin in GENEAread

sleep_combine_segment_data <- function(binfile,
                                 start_time,
                                 datacols,
                                 mmap.load = T,
                                 pagerefs) {
  
  # Naming of the csv file protocol.
  dataname <- naming_protocol(binfile)

  # Take the header file
  header <- header.info(binfile)

  # Finding the first time
  AccData1 <- read.bin(binfile, start = 0, end = 0.01, pagerefs = pagerefs)
  First_Time <- as.numeric(AccData1$data.out[1, 1])
  
  # Now I need to check that there is at least 24 hours of data
  # Last Time
  AccData2 <- read.bin(binfile, start = 0.99, end = 1, pagerefs = pagerefs)
  Last_Time <- as.numeric(AccData2$data.out[length(AccData2$data.out[, 1]), 1])
  
  DayNo <- as.numeric(ceiling((Last_Time - First_Time) / 86400))
  
  First_Start_Time  = as.POSIXct(First_Time, origin = "1970-01-01", tz = "GMT")
  First_Start_Time = as.Date(First_Start_Time)
  First_Start_time = as.POSIXct(paste(First_Start_Time, start_time), origin = "1970-01-01", tz = "GMT")
  First_Start_time = as.numeric(First_Start_time)
  
  # Is the start time before or after the first record time? 
  
  if (First_Time > First_Start_time){
    First_Start_time = First_Start_time + 86400
  } 
  
  if (Last_Time < First_Start_time + DayNo*86400){
    DayNo = DayNo - 1
    if (Last_Time < First_Start_time + DayNo*86400){
      DayNo = DayNo -1 
    }
  }

  # Initialise the segmented data
  segment_data <- c()
  # Break this into partial days -  needs to be based on start_time 

  for (i in 0:DayNo) {
    if (i == 0){
      segment_data1 <- getGENEAsegments(binfile,
                                        start = First_Time,
                                        end = First_Start_time,
                                        mmap.load = mmap.load,
                                        Use.Timestamps = TRUE,
                                        changepoint = "UpDownMeanVarDegreesMeanVar",
                                        penalty = "Manual",
                                        outputdir = "SleepClassification",
                                        pen.value1 = 40,
                                        pen.value2 = 400,
                                        datacols = datacols,
                                        intervalseconds = 30,
                                        mininterval = 1,
                                        downsample = as.numeric(unlist(header$Value[2])),
                                        pagerefs = pagerefs
                                        )
    } else if (i == DayNo){
      segment_data1 <- getGENEAsegments(binfile,
                                        start = First_Start_time + 86400 * (i - 1) ,
                                        end = Last_Time ,
                                        mmap.load = mmap.load,
                                        Use.Timestamps = TRUE,
                                        changepoint = "UpDownMeanVarDegreesMeanVar",
                                        penalty = "Manual",
                                        outputdir = "SleepClassification",
                                        pen.value1 = 40,
                                        pen.value2 = 400,
                                        datacols = datacols,
                                        intervalseconds = 30,
                                        mininterval = 1,
                                        downsample = as.numeric(unlist(header$Value[2])),
                                        pagerefs = pagerefs
                                        )
    } else {
      segment_data1 <- getGENEAsegments(binfile,
                                        start = First_Start_time + 86400 * (i - 1),
                                        end = First_Start_time + 86400 * i,
                                        mmap.load = mmap.load,
                                        Use.Timestamps = TRUE,
                                        changepoint = "UpDownMeanVarDegreesMeanVar",
                                        penalty = "Manual",
                                        outputdir = "SleepClassification",
                                        pen.value1 = 40,
                                        pen.value2 = 400,
                                        datacols = datacols,
                                        intervalseconds = 30,
                                        mininterval = 1,
                                        downsample = as.numeric(unlist(header$Value[2])),
                                        pagerefs = pagerefs
                                        )
    }
    
    segment_data <- rbind(segment_data, segment_data1)
    }
  
  # Add a date into this.
  segment_data$Date <- as.Date(as.POSIXct(as.numeric(segment_data$Start.Time), origin = "1970-01-01"))
  
  return(segment_data)
}

#### Testing file ####
# 
# binfile ="someGENEActiv.bin"
# 
# test_data = combine_segment_data(binfile,
#                                  start_time = "15:00",
#                                  datacols = c("UpDown.mean", "UpDown.var", "UpDown.sd",
#                                               "Degrees.mean", "Degrees.var", "Degrees.sd",
#                                               "Magnitude.mean", "Magnitude.var", 
#                                               "Magnitude.meandiff", "Magnitude.mad",
#                                               "Light.mean", "Light.max",
#                                               "Temp.mean", "Temp.sumdiff", 
#                                               "Temp.meandiff", "Temp.abssumdiff",
#                                               "Temp.sddiff", "Temp.var", 
#                                               "Temp.GENEAskew", "Temp.mad",
#                                               "Step.GENEAcount", "Step.sd", "Step.mean",
#                                               "Principal.Frequency.mean", "Principal.Frequency.median"),
#                                  mmap.load = T)


