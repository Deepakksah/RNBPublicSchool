using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Models;

namespace SchoolManagement.Data
{
    public static class HolidaySeeder
    {
        public static async Task SeedAllHolidaysAsync(ApplicationDbContext context)
        {
            var school = await context.Schools.FirstOrDefaultAsync(s => s.Code == "RNB-PIRO") 
                         ?? await context.Schools.FirstOrDefaultAsync();
            var schoolId = school?.Id ?? 4;

            var ay = await context.AcademicYears.FirstOrDefaultAsync(a => a.SchoolId == schoolId && a.IsCurrent)
                     ?? await context.AcademicYears.FirstOrDefaultAsync();
            var academicYearId = ay?.Id ?? 1;

            // Clear old demo holidays if any
            var existing = await context.Holidays.Where(h => h.SchoolId == schoolId || h.SchoolId == null).ToListAsync();
            if (existing.Count < 25)
            {
                context.Holidays.RemoveRange(existing);
                await context.SaveChangesAsync();

                var holidays = new List<Holiday>
                {
                    // April 2026
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Mahavir Jayanti", HolidayDate = new DateTime(2026, 4, 2), Description = "Gazetted Holiday — Birth of Lord Mahavira" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Good Friday", HolidayDate = new DateTime(2026, 4, 3), Description = "Gazetted Holiday — Christian Observance" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Eid-ul-Fitr", HolidayDate = new DateTime(2026, 4, 11), Description = "Gazetted Holiday — Festival of Eid" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Dr. B. R. Ambedkar Jayanti", HolidayDate = new DateTime(2026, 4, 14), Description = "National Holiday — Father of Indian Constitution" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Ram Navami", HolidayDate = new DateTime(2026, 4, 17), Description = "Gazetted Holiday — Birth of Lord Rama" },

                    // May 2026
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "International Labour Day", HolidayDate = new DateTime(2026, 5, 1), Description = "May Day / Shramik Diwas" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Buddha Purnima", HolidayDate = new DateTime(2026, 5, 12), Description = "Gazetted Holiday — Birth of Gautama Buddha" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Summer Vacation Begins", HolidayDate = new DateTime(2026, 5, 25), EndDate = new DateTime(2026, 6, 20), Description = "Annual Summer Break for Students" },

                    // June 2026
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Eid-ul-Adha (Bakrid)", HolidayDate = new DateTime(2026, 6, 17), Description = "Gazetted Holiday — Feast of Sacrifice" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Kabir Jayanti", HolidayDate = new DateTime(2026, 6, 22), Description = "State Holiday — Sant Kabir Das Birthday" },

                    // July 2026
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Muharram", HolidayDate = new DateTime(2026, 7, 17), Description = "Gazetted Holiday — Islamic New Year Observance" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Guru Purnima", HolidayDate = new DateTime(2026, 7, 21), Description = "School Observance & Teacher Homage Day" },

                    // August 2026
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Independence Day", HolidayDate = new DateTime(2026, 8, 15), Description = "National Holiday — 80th Independence Day Flag Hoisting" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Raksha Bandhan", HolidayDate = new DateTime(2026, 8, 27), Description = "Festival of Sibling Bond" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Chehlum", HolidayDate = new DateTime(2026, 8, 30), Description = "State Observance" },

                    // September 2026 (CURRENT MONTH)
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Shri Krishna Janmashtami", HolidayDate = new DateTime(2026, 9, 4), Description = "Gazetted Holiday — Birth of Lord Krishna" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Teacher's Day & Eid-e-Milad", HolidayDate = new DateTime(2026, 9, 5), Description = "Teacher's Day Celebration & Milad-un-Nabi Holiday" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Vishwakarma Puja", HolidayDate = new DateTime(2026, 9, 17), Description = "State Holiday — Divine Craftsman & Machinery Puja" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Anant Chaturdashi", HolidayDate = new DateTime(2026, 9, 25), Description = "Festival of Lord Vishnu & Ganesh Visarjan" },

                    // October 2026
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Mahatma Gandhi Jayanti", HolidayDate = new DateTime(2026, 10, 2), Description = "National Holiday — Father of the Nation Birthday" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Durga Puja (Maha Saptami)", HolidayDate = new DateTime(2026, 10, 19), EndDate = new DateTime(2026, 10, 22), Description = "Grand Durga Puja & Navratri Holidays" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Vijaya Dashami (Dussehra)", HolidayDate = new DateTime(2026, 10, 21), Description = "Gazetted Holiday — Victory of Good over Evil" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Maharishi Valmiki Jayanti", HolidayDate = new DateTime(2026, 10, 26), Description = "State Holiday — Author of Ramayana" },

                    // November 2026
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Dhanteras & Diwali Break", HolidayDate = new DateTime(2026, 11, 6), Description = "Auspicious Dhanteras Festival" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Diwali (Deepawali)", HolidayDate = new DateTime(2026, 11, 8), Description = "Gazetted Holiday — Festival of Lights" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Govardhan Puja & Bhai Dooj", HolidayDate = new DateTime(2026, 11, 9), EndDate = new DateTime(2026, 11, 10), Description = "Traditional Sibling & Annakut Festival" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Chhath Puja (Mahaparv)", HolidayDate = new DateTime(2026, 11, 15), EndDate = new DateTime(2026, 11, 18), Description = "Bihar's Mahaparv Chhath — Sandhya & Usha Arghya" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Guru Nanak Jayanti", HolidayDate = new DateTime(2026, 11, 24), Description = "Gazetted Holiday — Prakash Utsav" },

                    // December 2026
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Christmas Day", HolidayDate = new DateTime(2026, 12, 25), Description = "Gazetted Holiday — Christmas Celebration" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Winter Vacation Begins", HolidayDate = new DateTime(2026, 12, 26), EndDate = new DateTime(2027, 1, 2), Description = "Annual Winter Break for Students" },

                    // January 2027
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "New Year's Day", HolidayDate = new DateTime(2027, 1, 1), Description = "Welcome New Year 2027" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Guru Gobind Singh Jayanti", HolidayDate = new DateTime(2027, 1, 5), Description = "State Holiday — 10th Sikh Guru Prakash Utsav" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Makar Sankranti", HolidayDate = new DateTime(2027, 1, 14), Description = "Traditional Harvest Festival" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Netaji Subhash Bose Jayanti", HolidayDate = new DateTime(2027, 1, 23), Description = "Parakram Diwas" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Republic Day", HolidayDate = new DateTime(2027, 1, 26), Description = "National Holiday — 78th Republic Day Parade & Celebration" },

                    // February 2027
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Vasant Panchami (Saraswati Puja)", HolidayDate = new DateTime(2027, 2, 11), Description = "Grand School Saraswati Puja & Cultural Ceremony" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Maha Shivratri", HolidayDate = new DateTime(2027, 2, 16), Description = "Gazetted Holiday — Lord Shiva Worship" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Guru Ravidas Jayanti", HolidayDate = new DateTime(2027, 2, 21), Description = "State Observance Holiday" },

                    // March 2027
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Holika Dahan", HolidayDate = new DateTime(2027, 3, 21), Description = "Eve of Holi Bonfire" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Holi (Dhulandi)", HolidayDate = new DateTime(2027, 3, 22), Description = "Gazetted Holiday — Grand Festival of Colors" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Bihar Diwas", HolidayDate = new DateTime(2027, 3, 22), Description = "Bihar Statehood Day Special Celebration" },
                    new Holiday { SchoolId = schoolId, AcademicYearId = academicYearId, Name = "Good Friday", HolidayDate = new DateTime(2027, 3, 26), Description = "Gazetted Christian Observance" }
                };

                context.Holidays.AddRange(holidays);
                await context.SaveChangesAsync();
            }
        }
    }
}
