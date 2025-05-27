# ==================================
# CONFIGURATION
# ==================================

source("utils/config.R")

cleanup_classification()

# ==================================
# MAIN LOGIC
# ==================================

files <- getBinFiles()

for (binfile in files) {
  
  my_pagerefs <- getPages(binfile)

  if (analyze_activity) {
    activity_analysis(
      binfile = binfile,
      summary_name = getSummaryName("Activity_Summary_Metrics_"),
      pagerefs = my_pagerefs
    )
  }

  if (analyze_sleep) {
    sleep_analysis(
      binfile = binfile,
      summary_name = getSummaryName("Sleep_Summary_Metrics_"),
      pagerefs = my_pagerefs
    )
  }
  
}

# ==================================
# END OF PROGRAM
# ==================================

if (use_clipping) { clipEndings() }

cleanup_classification()

