activity_analysis <- function(binfile, summary_name, timer = FALSE) {
  
  # --- TIMER VALUES ---
  
  #timer = params$timer
  # timer = TRUE # For Manual Testing and vebrose mode remove comment
  if (timer) {
    times = time_stage = c()
    times = append(times, Sys.time())  
    time_stage = append(time_stage, paste0("Start of File"))
  }
  
  # --- R PARAMETERS ---
  
  mmap.load = TRUE
  datacols = c(
    "UpDown.mean", "UpDown.var", "UpDown.mad",
    "Degrees.mean", "Degrees.var", "Degrees.mad",
    "Magnitude.mean",
    "Light.mean",
    "Temp.mean", "Temp.sumdiff", "Temp.abssumdiff",
    "Step.GENEAcount", "Step.sd", "Step.mean", "Step.median"
  )
  
  start_time = "03:00"
  
  # --- MANUAL PARAMETERS ---
  library(GENEAread)
  library(GENEAclassify)
  library(ggplot2)
  library(scales)
  library(reshape2)
  
  # Functions to use.
  source("utils/01_library_installer.R")
  source("utils/02_activity_combine_segment_data.R")
  source("utils/03_naming_protocol.R")
  source("utils/04_activity_create_df_pcp.R")
  source("utils/05_number_of_days.R")
  source("utils/06_bed_rise_detect.R")
  source("utils/07_activity_state_rearrange.R")
  source("utils/08_activity_summary.R")
  
  i = 1
  BinPattern = "*\\.[bB][iI][nN]$"
  files = list.files(path = paste0(getwd(), "/data"), pattern = BinPattern, full.names = TRUE)
  binfile = files[i]
  header = header.info(binfile)
  summary_name = paste0("Activity_Summary_Metrics_", strsplit(unlist(strsplit(binfile, "/"))[length(unlist(strsplit(binfile, "/")))], ".bin")[[1]])
  mmap.load = FALSE
  datacols = c(
    "UpDown.mean", "UpDown.var", "UpDown.mad",
    "Degrees.mean", "Degrees.var", "Degrees.mad",
    "Magnitude.mean",
    "Light.mean",
    "Temp.mean", "Temp.sumdiff", "Temp.abssumdiff",
    "Step.GENEAcount", "Step.sd", "Step.mean", "Step.median"
  )
  
  
  start_time = "03:00"
  
  
  # --- COMBINING AND CLASSIFYING SEGMENTED DATA ---

  if (timer) {
    # Starting the timer
    cat("
        #. Start of Segmenting data")
    print(Sys.time())
    times = append(times, Sys.time())
    time_stage = append(time_stage, paste0("Start of Segmenting data"))
  }
  
  data_name = naming_protocol(binfile, prefix = "")
  
  # Segment the data using the Awake Sleep Model.
  segment_data = combine_segment_data(binfile,
                                      start_time,
                                      datacols,
                                      mmap.load = mmap.load
  )
  
  # Routine to remove overlap segments - Can turn this into a function
  k = 1 # Counter
  collen = length(segment_data$Start.Time)
  overlap_list = which(segment_data$Start.Time[1:(collen - k)] > segment_data$Start.Time[(k+1):collen])
  
  while (length(overlap_list) > 0){
    segment_data = segment_data[-overlap_list,]
    k = k + 1 
    overlap_list = which(segment_data$Start.Time[1:(collen - (k))] > segment_data$Start.Time[(k+1):collen])
  }
  
  data_name = naming_protocol(binfile)
  
  # DELETE THIS MECHANISM, ALTHOUGH "readRDS" is used in "combine_segment_data.R", therefore this needs to be checked first.
  saveRDS(segment_data, file.path(getwd(), "/outputs/", data_name))
  
  # Write out the version of data that we want people to see
  #csvname = naming_protocol(binfile, suffix = "_All_Data.csv")
  
  #write.csv(segment_data,
  #  file = file.path(paste0("outputs/", csvname))
  #)
  
  # --- ACTIVITY_CREATE_DF_PCP FUNCTION ---
  
  if (timer) {
    # Starting the timer
    cat("
        #. Deciding on classes")
    print(Sys.time())
    times = append(times, Sys.time())
    time_stage = append(time_stage, paste0("Deciding on Classes"))
  }
  
  header = header.info(binfile)
  
  df_pcp = activity_create_df_pcp(segment_data,
    summary_name,
    header = header,
    # r Cut_points
    Magsa_cut = 0.04, # 40mg
  
    Duration_low  = exp(5.5),
    Duration_high = exp(8),
  
    Mad.Score2_low  = 0.45,
    Mad.Score2_high = 5,
  
    Mad.Score4_low  = 0.45,
    Mad.Score4_high = 5,
  
    Mad.Score6_low  = 0.45,
    Mad.Score6_high = 5,
  
    Mad_low  = 0.45,
    Mad_high = 5
  )
  
  segment_data1 = df_pcp
  # Add the additional class onto the end.
  segment_data1$Class.prior = segment_data1$Class.current = segment_data1$Class.post = 2
  # Create the correct classes for post and priors.
  segment_data1$Class.prior[1:(length(segment_data1$Class.prior) - 2)] = df_pcp$Class.current
  segment_data1$Class.current[2:(length(segment_data1$Class.prior) - 1)] = df_pcp$Class.current
  segment_data1$Class.post[3:(length(segment_data1$Class.prior))] = df_pcp$Class.current

  
  # --- NUMBER OF DAYS FUNCTION ---

  # no_days = number_of_days(binfile,  start_time) # This is crashing the markdown for me! 
  file_times = segment_data$Start.Time
  first_time = file_times[1]
  last_time = file_times[length(file_times)]
  no_days = as.numeric(ceiling((last_time - first_time) / 86400)) + 1
  
  
  # --- BED RISE DETECTION ---
  
  if (timer) {
    # Starting the timer
    cat("
        #. Start of Bed Rise Algorithm")
    print(Sys.time())
    times = append(times, Sys.time())
    time_stage = append(time_stage, paste0("Start of Bed Rise Algorithm"))
  }
  
  # Find the Bed and Rise times
  bed_rise_df = bed_rise_detect(binfile,
                                 df_pcp,
                                 no_days,
                                 verbose = FALSE
  )
  
  # --- INITIALISIERUNG PARAMETERS ---

  t = as.POSIXct(as.numeric(as.character(segment_data1$Start.Time[1])), origin = "1970-01-01")
  first_date = as.Date(t)
  first_time = as.POSIXct(as.character(paste((first_date - 1), "15:00")), format = "%Y-%m-%d %H:%M", origin = "1970-01-01")
  
  min_boundary = max_boundary = c()
  
  for (i in 1:(no_days)) {
    min_boundary[i] = as.POSIXct(as.character(paste(first_date + i - 1, start_time)), format = "%Y-%m-%d %H:%M", origin = "1970-01-01")
    max_boundary[i] = as.POSIXct(as.character(paste(first_date + i, start_time)), format = "%Y-%m-%d %H:%M", origin = "1970-01-01")
  }
  
  boundarys = cbind(min_boundary, max_boundary)
  
  header = header.info(binfile)
  
  # --- STATE REARRANGE ---
  
  # Use the Bed Rise rules to determine what state this will
  segment_data1 = activity_state_rearrange(
    segment_data1,
    boundarys,
    bed_rise_df$bed_time,
    bed_rise_df$rise_time,
    first_date
  )
  
  # Write out the version of data that we want people to see
  # csvname = naming_protocol(binfile, prefix = "", suffix = "_All_Data.csv")
  # 
  # write.csv(segment_data1,
  #   file = file.path(paste0("outputs/", csvname))
  # )
  
  # --- ACTIVITY STATS TABLE ---
  
  activity_df = activity_detect(
    segment_data1,
    boundarys
  )
  
  write.csv(activity_df, file.path(paste0("outputs/", summary_name, ".csv")), row.names = FALSE)
  
  # --- OUTPUTTING TIMER CSV ---
  if (timer){
        cat("
          #. End of analysis for file ", binfile)
      print(Sys.time())
    times = as.numeric(append(times, Sys.time()))
    time_stage = append(time_stage, paste0("End of File"))
    timing_csv_name = naming_protocol(binfile, prefix = "", suffix = "_Time_Analysis_Report.csv")
    time_df = data.frame(times, time_stage)
    write.csv(time_df, timing_csv_name)
  }

}
