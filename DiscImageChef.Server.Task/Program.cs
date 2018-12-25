using System;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using DiscImageChef.Server.Migrations;
using DiscImageChef.Server.Models;

namespace DiscImageChef.Server.Task
{
    class Program
    {
        public static void Main(string[] args)
        {
            DateTime start, end;
            Console.WriteLine("{0}: Migrating database to latest version...", DateTime.UtcNow);
            start = DateTime.UtcNow;
            Configuration migratorConfig = new Configuration();
            DbMigrator    dbMigrator     = new DbMigrator(migratorConfig);
            dbMigrator.Update();
            end = DateTime.UtcNow;
            Console.WriteLine("{0}: Took {1:F2} seconds", DateTime.UtcNow, (end - start).TotalSeconds);

            start = DateTime.UtcNow;
            Console.WriteLine("{0}: Connecting to database...", DateTime.UtcNow);
            DicServerContext ctx = new DicServerContext();
            end = DateTime.UtcNow;
            Console.WriteLine("{0}: Took {1:F2} seconds", end, (end - start).TotalSeconds);

            try
            {
                Console.WriteLine("{0}: Retrieving USB IDs from Linux USB...", DateTime.UtcNow);
                start = DateTime.UtcNow;
                WebClient    client = new WebClient();
                StringReader sr     = new StringReader(client.DownloadString("http://www.linux-usb.org/usb.ids"));
                end = DateTime.UtcNow;
                Console.WriteLine("{0}: Took {1:F2} seconds", end, (end - start).TotalSeconds);

                UsbVendor vendor           = null;
                int       newVendors       = 0;
                int       newProducts      = 0;
                int       modifiedVendors  = 0;
                int       modifiedProducts = 0;
                int       counter          = 0;

                start = DateTime.UtcNow;
                Console.WriteLine("{0}: Adding and updating database entries...", DateTime.UtcNow);
                do
                {
                    if(counter == 1000)
                    {
                        DateTime start2 = DateTime.UtcNow;
                        Console.WriteLine("{0}: Saving changes", start2);
                        ctx.SaveChanges();
                        end = DateTime.UtcNow;
                        Console.WriteLine("{0}: Took {1:F2} seconds", end, (end - start2).TotalSeconds);
                        counter = 0;
                    }

                    string line = sr.ReadLine();

                    if(line is null) break;

                    if(line.Length == 0 || line[0] == '#') continue;

                    ushort number;
                    string name;
                    if(line[0] == '\t')
                    {
                        try { number = Convert.ToUInt16(line.Substring(1, 4), 16); }
                        catch(FormatException) { continue; }

                        if(number == 0) continue;

                        name = line.Substring(7);

                        UsbProduct product =
                            ctx.UsbProducts.FirstOrDefault(p => p.ProductId       == number && p.Vendor != null &&
                                                                p.Vendor.VendorId == vendor.VendorId);

                        if(product is null)
                        {
                            product = new UsbProduct(vendor, number, name);
                            ctx.UsbProducts.Add(product);
                            Console.WriteLine("{0}: Will add product {1} with ID {2:X4} and vendor {3} ({4:X4})",
                                              DateTime.UtcNow, product.Product, product.ProductId,
                                              product.Vendor?.Vendor ?? "null", product.Vendor?.VendorId ?? 0);
                            newProducts++;
                            counter++;
                        }
                        else if(name != product.Product)
                        {
                            Console
                               .WriteLine("{0}: Will modify product with ID {1:X4} and vendor {2} ({3:X4}) from \"{4}\" to \"{5}\"",
                                          DateTime.UtcNow, product.ProductId, product.Vendor?.Vendor ?? "null",
                                          product.Vendor?.VendorId                                   ?? 0,
                                          product.Product, name);
                            product.Product      = name;
                            product.ModifiedWhen = DateTime.UtcNow;
                            modifiedProducts++;
                            counter++;
                        }

                        continue;
                    }

                    try { number = Convert.ToUInt16(line.Substring(0, 4), 16); }
                    catch(FormatException) { continue; }

                    if(number == 0) continue;

                    name = line.Substring(6);

                    vendor = ctx.UsbVendors.FirstOrDefault(v => v.VendorId == number);

                    if(vendor is null)
                    {
                        vendor = new UsbVendor(number, name);
                        ctx.UsbVendors.Add(vendor);
                        Console.WriteLine("{0}: Will add vendor {1} with ID {2:X4}", DateTime.UtcNow, vendor.Vendor,
                                          vendor.VendorId);
                        newVendors++;
                        counter++;
                    }
                    else if(name != vendor.Vendor)
                    {
                        Console.WriteLine("{0}: Will modify vendor with ID {1:X4} from \"{2}\" to \"{3}\"",
                                          DateTime.UtcNow, vendor.VendorId, vendor.Vendor, name);
                        vendor.Vendor       = name;
                        vendor.ModifiedWhen = DateTime.UtcNow;
                        modifiedVendors++;
                        counter++;
                    }
                }
                while(true);

                end = DateTime.UtcNow;
                Console.WriteLine("{0}: Took {1:F2} seconds", end, (end - start).TotalSeconds);

                Console.WriteLine("{0}: Saving database changes...", DateTime.UtcNow);
                start = DateTime.UtcNow;
                ctx.SaveChanges();
                end = DateTime.UtcNow;
                Console.WriteLine("{0}: Took {1:F2} seconds", end, (end - start).TotalSeconds);

                Console.WriteLine("{0}: {1} vendors added.",     DateTime.UtcNow, newVendors);
                Console.WriteLine("{0}: {1} products added.",    DateTime.UtcNow, newProducts);
                Console.WriteLine("{0}: {1} vendors modified.",  DateTime.UtcNow, modifiedVendors);
                Console.WriteLine("{0}: {1} products modified.", DateTime.UtcNow, modifiedProducts);

                Console.WriteLine("{0}: Looking up a vendor", DateTime.UtcNow);
                start  = DateTime.UtcNow;
                vendor = ctx.UsbVendors.FirstOrDefault(v => v.VendorId == 0x8086);
                if(vendor is null) Console.WriteLine("{0}: Error, could not find vendor.", DateTime.UtcNow);
                else
                    Console.WriteLine("{0}: Found {1}.", DateTime.UtcNow,
                                      vendor.Vendor);
                end = DateTime.UtcNow;
                Console.WriteLine("{0}: Took {1:F2} seconds", end, (end - start).TotalSeconds);

                Console.WriteLine("{0}: Looking up a product", DateTime.UtcNow);
                start = DateTime.UtcNow;
                UsbProduct prd =
                    ctx.UsbProducts.FirstOrDefault(p => p.ProductId == 0x0001 && p.Vendor.VendorId == 0x8086);
                if(prd is null) Console.WriteLine("{0}: Error, could not find product.", DateTime.UtcNow);
                else Console.WriteLine("{0}: Found {1}.",                                DateTime.UtcNow, prd.Product);
                end = DateTime.UtcNow;
                Console.WriteLine("{0}: Took {1:F2} seconds", end, (end - start).TotalSeconds);
            }
            catch(Exception ex)
            {
                #if DEBUG
                if(Debugger.IsAttached) throw;
                #endif
                Console.WriteLine("{0}: Exception {1} filling USB IDs...", DateTime.UtcNow, ex);
            }

            Console.WriteLine("{0}: Fixing all devices without modification time...", DateTime.UtcNow);
            start = DateTime.UtcNow;
            foreach(Device device in ctx.Devices.Where(d => d.ModifiedWhen == null))
                device.ModifiedWhen = device.AddedWhen;
            end = DateTime.UtcNow;
            Console.WriteLine("{0}: Took {1:F2} seconds", end, (end - start).TotalSeconds);

            Console.WriteLine("{0}: Committing changes...", DateTime.UtcNow);
            start = DateTime.UtcNow;
            ctx.SaveChanges();
            end = DateTime.UtcNow;
            Console.WriteLine("{0}: Took {1:F2} seconds", end, (end - start).TotalSeconds);
        }
    }
}