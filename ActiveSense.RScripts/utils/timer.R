# ==================================
# TIMER CLASS
# ==================================

create_timer <- function(name) {
  obj <- list(
    name = name,
    timer_data = data.frame(
      name = character(0),
      time = numeric(0)
    )
  )
  
  class(obj) <- "timer"
  
  return(obj)
}

append.timer <- function(self, stage) {
  new_row <- data.frame(name = stage, time = Sys.time())
  
  self$timer_data <- rbind(self$timer_data, new_row)
  
  return(self)
}

translate.timer <- function(df) {
  df$time_diff <- NA
  
  for (i in 1:nrow(df)-1) {
    time_difference <- as.numeric(difftime(df[i+1, "time"], df[i, "time"], units = "secs"))
    
    df[i, "time_diff"] <- time_difference
  }
  
  return(df)
}

# ==================================
# HELPER FUNCTIONS
# ==================================

timer_merge <- function(timer_list, binfile) {
  result_df <- data.frame()
  
  for (el in timer_list) {
    
    print("PRINTING VECTOR")
    print(el)
    print("Finalized")
    
    df <- el$timer_data
    df <- translate.timer(df)
    
    new_row <- data.frame(name = el$name, time = "", time_diff = "", stringsAsFactors = FALSE)
    
    result_df <- rbind(result_df, new_row, df)
  }
  
  timing_csv_name = naming_protocol(binfile, prefix = "", suffix = "_Time_Analysis_Report.csv")
  write.csv(result_df, timing_csv_name)
}
