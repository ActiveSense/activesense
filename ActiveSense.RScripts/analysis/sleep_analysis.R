sleep_analysis <- function(binfile, summary_name, timer = FALSE) {

  if (timer) {
    times = time_stage = c()
    times = append(times, Sys.time())
    time_stage = append(time_stage, paste0("Start of File"))
  }
  
  # --- R PARAMETERS ---
  UpDown.mad_plot_switch = TRUE
  daily_plot_switch = TRUE
  daily_summary_switch = TRUE
  device_details_switch = TRUE
  sleep_interruptions_switch = FALSE
  mmap.load = FALSE
  datacols = c(
    "UpDown.mean", "UpDown.var", "UpDown.mad",
    "Degrees.mean", "Degrees.var", "Degrees.mad",
    "Magnitude.mean",
    "Light.mean",
    "Temp.mean", "Temp.sumdiff", "Temp.abssumdiff",
    "Step.GENEAcount", "Step.sd", "Step.mean", "Step.median",
    "Principal.Frequency.mean", "Principal.Frequency.median"
  )
  
  start_time = "15:00"
  
  # --- LOADING PACKAGES ---
  
  # Change eval to FALSE when testing
  # make sure that the latest versions are loaded from CRAN
  # run sessionInfo() to check which packages are loaded
  # and make sure that they are the latest version numbers.
  # Parameters passed to the packages will be different for each version.
  # This can cause programs to return incorrect results and errors
  
  library(GENEAread)
  library(GENEAclassify)
  library(ggplot2)
  library(scales)
  library(reshape2)
  
  # utils to use.
  # Although these functions are named in this list with a prefix number,
  # This is so that the functions show in the folder in the order that they are used.
  # When they are called then the internal function name 
  # does not contain the number
  # ie 02_combine_segment_data is called using combine_segment_data
  
  source("utils/01_library_installer.R")
  source("utils/02_sleep_combine_segment_data.R")
  source("utils/03_naming_protocol.R")
  source("utils/04_sleep_create_df_pcp.R")
  source("utils/05_number_of_days.R")
  source("utils/06_bed_rise_detect.R")
  source("utils/07_sleep_state_rearrange.R")
  source("utils/08_sleep_summary.R")
  
  i = 1
  file_pattern = "*\\.[bB][iI][nN]$"
  files = list.files(path = paste0(getwd(), "/data"), pattern = file_pattern, full.names = TRUE)
  binfile = files[i]
  summary_name = paste0("Sleep_Summary_Metrics_", strsplit(unlist(strsplit(binfile, "/"))[length(unlist(strsplit(binfile, "/")))], ".bin")[[1]])
  timer = TRUE
  datacols = c(
    "UpDown.mean", "UpDown.var", "UpDown.sd",
    "Degrees.mean", "Degrees.var", "Degrees.sd",
    "Magnitude.mean", "Magnitude.var", "Magnitude.meandiff", "Magnitude.mad",
    "Light.mean", "Light.max",
    "Temp.mean", "Temp.sumdiff", "Temp.meandiff", "Temp.abssumdiff",
    "Temp.sddiff", "Temp.var", "Temp.GENEAskew", "Temp.mad",
    "Step.GENEAcount", "Step.sd", "Step.mean", 
    "Principal.Frequency.mean", "Principal.Frequency.median"
  )
  
  start_time = "15:00"
  
  # --- COMBINING AND CLASSIFYING SEGMENTED DATA ---
  # This section segments the data into whole 24 hour periods and partial periods,
  # ready to calculate the information correctly
  if (timer) {
    # Starting the timer
    cat("
      #. Start of Segmenting data")
    print(Sys.time())
    times = time_stage = c()
    times = append(times, Sys.time())
    time_stage = append(time_stage, paste0("Start of Segmenting data"))
  }
  
  # Segment the data.
  segment_data = combine_segment_data(binfile,
                                      start_time,
                                      datacols,
                                      mmap.load = mmap.load
  )
  
  
  # Routine to remove overlap segments.
  k = 1 # Counter
  collen = length(segment_data$Start.Time)
  overlap_list = which(segment_data$Start.Time[1:(collen - k)] > segment_data$Start.Time[(k+1):collen])
  
  while (length(overlap_list) > 0){
    segment_data = segment_data[-overlap_list,]
    k = k + 1 
    overlap_list = which(segment_data$Start.Time[1:(collen - (k))] > segment_data$Start.Time[(k+1):collen])
  }
  
  data_name = naming_protocol(binfile, prefix = "Sleep_")
  
  saveRDS(segment_data, file.path(getwd(), "/outputs/", data_name))
  
  # Write out the version of data that we want people to see
  csvname = naming_protocol(binfile, prefix = "Sleep_", suffix = "_All_Data.csv")
  
  #TODO: Delete this file alltogehter. Not needed anymore.
  #write.csv(segment_data, file = file.path(paste0("outputs/", csvname)))
  
  # --- CREATE DF PCP ---
  if (timer) {
    # Starting the timer
    cat("
      #. Deciding on classes")
    print(Sys.time())
    times = append(times, Sys.time())
    time_stage = append(time_stage, paste0("Deciding on Classes"))
  }
  
  df_pcp = create_df_pcp(segment_data,
                         summary_name,
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
  
  # Add the additional class onto the end.
  segment_data$Class.prior = segment_data$Class.current = segment_data$Class.post = 2
  # Create the correct classes for post and priors.
  segment_data$Class.prior[1:(length(segment_data$Class.prior) - 2)] = df_pcp$Class.current
  segment_data$Class.current[2:(length(segment_data$Class.prior) - 1)] = df_pcp$Class.current
  segment_data$Class.post[3:(length(segment_data$Class.prior))] = df_pcp$Class.current
  
  # Once we've created a sleep score write out the data here. No longer need to save this
  saveRDS(df_pcp,
          file = file.path(paste0("outputs/", summary_name, "pcp.rds")))
  
  # --- NUMBER OF DAYS ---
  # no_days = number_of_days(binfile,  start_time)
  # This needs to take into account partial days at the beginning and end of the data
  file_times = segment_data$Start.Time
  first_time = file_times[1]
  last_time = file_times[length(file_times)]
  no_days = as.numeric(ceiling((last_time - first_time) / 86400))
  
  # --- BED RISE DETECTION ---
  
  if (timer) {
    # Starting the timer
    cat("
      #. Start of Bed Rise Algorithm")
    print(Sys.time())
    times = append(times, Sys.time())
    time_stage = append(time_stage, paste0("Start of Bed Rise Algorithm"))
  }
  
  # TODO: We don't have a sleep diary. Kill this.
  # Now check to see if there is a sleep_diary 
  Sleep_Diary = NA
  
  # try({
  #   # Checking to find if there is a corresponding sleep diary 
  #   binfile_stripped = unlist(strsplit(binfile, "/"))
  #   binfile_stripped = binfile_stripped[length(binfile_stripped)]
  #   binfile_stripped = unlist(strsplit(binfile_stripped, ".bin"))
  #   
  #   Sleep_Diary = read.csv(paste0("Sleep_Diaries/", binfile_stripped, "_Sleep_Diary.csv"))
  # })
  
  # Find the Bed and Rise times
  bed_rise_df = bed_rise_detect(binfile,
                                df_pcp,
                                no_days,
                                Sleep_Diary,
                                verbose = FALSE
  )
  
  # --- INITIALISIERUNG PARAMETERS ---
  
  t = as.POSIXct(as.numeric(as.character(segment_data$Start.Time[1])), origin = "1970-01-01")
  first_date = as.Date(t)
  min_boundary = max_boundary = c()
  
  for (i in 1:(no_days)) {
    min_boundary[i] = as.POSIXct(as.character(paste(first_date + i - 1, start_time)), format = "%Y-%m-%d %H:%M", origin = "1970-01-01")
    max_boundary[i] = as.POSIXct(as.character(paste(first_date + i, start_time)), format = "%Y-%m-%d %H:%M", origin = "1970-01-01")
  }
  
  boundarys = cbind(min_boundary, max_boundary)
  
  header = header.info(binfile)
  
  # --- STATE REARRANGE ---
  
  # Use the Bed Rise rules to determine what state this will
  segment_data = state_rearrange(
    segment_data,
    boundarys,
    bed_rise_df$bed_time,
    bed_rise_df$rise_time,
    first_date
  )
  
  # # Write out the version of data that we want people to see
  # csvname = naming_protocol(binfile, suffix = "_States_Added_All_Data.csv")
  # 
  # write.csv(segment_data,
  #   file = file.path(paste0("outputs/", csvname))
  # )
  
  
  # --- SUMMARY ---
  
  if (daily_summary_switch == TRUE) {
    
    statistics = sleep_summary(bed_rise_df,
                               first_date)
    
    write.csv(statistics, file.path(paste0("outputs/", summary_name, ".csv")), row.names = FALSE)
    
  }
  
  
  # --- OUTPUTTING TIMER CSV ---
  
  # Outputting additional information to identify issues
  if (timer){
    cat("
        #. End of analysis for file ", binfile)
    print(Sys.time())
    times = as.numeric(append(times, Sys.time()))
    time_stage = append(time_stage, paste0("End of File"))
    timing_csv_name = naming_protocol(binfile, prefix = "Sleep_", suffix = "_Time_Analysis_Report.csv")
    time_df = data.frame(times, time_stage)
    write.csv(time_df, timing_csv_name)
  }
  
}

