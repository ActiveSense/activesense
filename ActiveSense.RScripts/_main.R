# ==================================
# CONFIGURATION
# ==================================

source("utils/config.R")

if (timer) {
  timer_total <- create_timer("Total Timer")
  timer_total <- append.timer(timer_total, "Start of the analysis") 
}

# ==================================
# MAIN LOGIC
# ==================================

num_cores <- parallel::detectCores() - 1
plan(multisession, workers = num_cores)

files <- getBinFiles()

future_tasks <- list()

for (binfile in files) {
  
  if (analyze_activity) {
    future_tasks[[paste0(binfile, "_activity")]] <- future({
      activity_analysis(
        binfile = binfile,
        summary_name = getSummaryName("Activity_Summary_Metrics_")
      )
    })
  }
  
  if (analyze_sleep) {
    future_tasks[[paste0(binfile, "_sleep")]] <- future({
      sleep_analysis(
        binfile = binfile,
        summary_name = getSummaryName("Sleep_Summary_Metrics_")
      )
    })
  }
  
}

timer_results <- list()
for (task_name in names(future_tasks)) {
  timer_results[[task_name]] <- value(future_tasks[[task_name]])
}

plan(sequential)

# ==================================
# END OF PROGRAM // TIMER END
# ==================================

cleanup_classification()

if (timer) {
  timer_total <- append.timer(timer_total, "End of Analyis")
  
  timer_results <- c(list(timer_total), timer_results)

  timer_merge(timer_results, binfile)
}

