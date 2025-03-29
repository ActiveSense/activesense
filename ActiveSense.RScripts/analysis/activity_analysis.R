activity_analysis <- function(binfile, summary_name) {
  
  # ==================================
  # CONFIGURATION
  # ==================================
  
  datacols <- activity_datacols
  
  start_time = "03:00"

  header = header.info(binfile)
  
  # ==================================
  # DATA SEGMENTATION 
  # ==================================
  
  if (timer) {
    my_timer <- create_timer("Activity Timer")
    my_timer <- append.timer(my_timer, "Segmentation") 
  }
  
  # Segment the data using the Awake Sleep Model.
  segment_data = activity_combine_segment_data(binfile,
                                      start_time,
                                      datacols,
                                      mmap.load = mmap.load
  )
  
  cleanup_classification("ActivityClassification")
  
  # Routine to remove overlap segments
  k = 1 # Counter
  collen = length(segment_data$Start.Time)
  overlap_list = which(segment_data$Start.Time[1:(collen - k)] > segment_data$Start.Time[(k+1):collen])
  
  while (length(overlap_list) > 0){
    segment_data = segment_data[-overlap_list,]
    k = k + 1 
    overlap_list = which(segment_data$Start.Time[1:(collen - (k))] > segment_data$Start.Time[(k+1):collen])
  }
  
  # ==================================
  # DATA CLASSIFICATION
  # ==================================

  if (timer) my_timer <- append.timer(my_timer, "Classification")
  
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

  
  # ==================================
  # NUMBER OF DAYS
  # ==================================

  # no_days = number_of_days(binfile,  start_time) # This is crashing the markdown for me! 
  file_times = segment_data$Start.Time
  first_time = file_times[1]
  last_time = file_times[length(file_times)]
  no_days = as.numeric(ceiling((last_time - first_time) / 86400)) + 1
  
  
  # ==================================
  # BED RISE DETECTION
  # ==================================
  
  if (timer) my_timer <- append.timer(my_timer, "Bed Rise Algorithm")
  
  # Find the Bed and Rise times
  bed_rise_df = bed_rise_detect(binfile,
                                 df_pcp,
                                 no_days,
                                 verbose = FALSE
  )
  
  # ==================================
  # INITIALISIERUNG PARAMETERS
  # ==================================

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
  
  # ==================================
  # STATE REARRANGE
  # ==================================
  
  # Use the Bed Rise rules to determine what state this will
  segment_data1 = activity_state_rearrange(
    segment_data1,
    boundarys,
    bed_rise_df$bed_time,
    bed_rise_df$rise_time,
    first_date
  )
  
  # ==================================
  # ACTIVITY SUMMARY TABLE
  # ==================================
  
  activity_df = activity_detect(
    segment_data1,
    boundarys
  )
  
  write.csv(activity_df, file.path(paste0("outputs/", summary_name, ".csv")), row.names = FALSE)
  
  # ==================================
  # END OF SCRIPT // TIMER
  # ==================================
  if (timer) {
    my_timer <- append.timer(my_timer, "End of activity analysis")
    return(my_timer)
  }
  
}
